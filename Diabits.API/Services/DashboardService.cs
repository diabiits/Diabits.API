using System.Collections;

using Diabits.API.Data;
using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Helpers.Mapping;
using Diabits.API.Interfaces;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Diabits.API.Models.HealthDataPoints.ManualInput;

using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Services;

public class DashboardService : IDashboardService
{
    private readonly DiabitsDbContext _dbContext;
    private readonly MapperFactory _mapperFactory;

    public DashboardService(DiabitsDbContext dbContext, MapperFactory mapperFactory)
    {
        _dbContext = dbContext;
        _mapperFactory = mapperFactory;
    }

    public async Task<TimelineResponse> GetTimelineAsync(string userId, DateTime date, int bucketMinutes)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddMilliseconds(-1);

        // Fetch full entities to use mappers
        var glucose = await _dbContext.Set<GlucoseLevel>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var heartRate = await _dbContext.Set<HeartRate>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var workouts = await _dbContext.Set<Workout>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        //TODO Duration in minutes 
        var sleeps = await _dbContext.Set<SleepSession>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var medications = await _dbContext.Set<Medication>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var menstruation = await _dbContext.Set<Menstruation>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .FirstOrDefaultAsync();

        // Convert to DTOs using mappers
        var glucoseDtos = glucose.Select(g => (NumericDto)_mapperFactory.ToDto(g)).ToList();
        var heartRateDtos = heartRate.Select(hr => (NumericDto)_mapperFactory.ToDto(hr)).ToList();
        var sleepDtos = sleeps.Select(s => (NumericDto)_mapperFactory.ToDto(s)).ToList();
        var workoutDtos = workouts.Select(w => (WorkoutDto)_mapperFactory.ToDto(w)).ToList();
        var medicationDtos = medications.Select(m => (ManualInputDto)_mapperFactory.ToDto(m)).ToList();
        var menstruationDto = menstruation != null ? (ManualInputDto)_mapperFactory.ToDto(menstruation) : null;

        // Bucket numeric data for charting
        var glucoseSeries = BucketNumericSeries(glucoseDtos, start, bucketMinutes, decimals: 1);
        var heartRateSeries = BucketNumericSeries(heartRateDtos, start, bucketMinutes, decimals: 0);

        return new TimelineResponse(
            GlucoseLevels: glucoseSeries,
            HeartRates: heartRateSeries,
            SleepSessions: sleepDtos,
            Workouts: workoutDtos,
            Medications: medicationDtos,
            Menstruation: menstruationDto
        );
    }

    private static List<NumericDto> BucketNumericSeries(
        IReadOnlyList<NumericDto> points,
        DateTime dayStart,
        int bucketMinutes,
        int decimals)
    {
        if (bucketMinutes <= 0 || 1440 % bucketMinutes != 0)
            throw new ArgumentOutOfRangeException(nameof(bucketMinutes));

        var bucketsPerDay = 1440 / bucketMinutes;

        var grouped = points
            .GroupBy(p => (int)((p.DateFrom - dayStart).TotalMinutes / bucketMinutes))
            .Where(g => g.Key >= 0 && g.Key < bucketsPerDay)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Average = Math.Round(g.Average(x => x.HealthValue.NumericValue), decimals),
                    Time = dayStart.AddMinutes(g.Key * bucketMinutes),
                    Type = g.First().HealthDataType
                }
            );

        var result = new List<NumericDto>(grouped.Count);

        for (var i = 0; i < bucketsPerDay; i++)
        {
            if (grouped.TryGetValue(i, out var bucket))
            {
                result.Add(new NumericDto
                {
                    DateFrom = bucket.Time,
                    HealthDataType = bucket.Type,
                    HealthValue = new NumericHealthValueDto { NumericValue = bucket.Average }
                });
            }
        }

        return result;
    }
}


// Timeline response reuses existing DTOs but provides bucketed data for glucose/heart rate
public record TimelineResponse(
    // Bucketed numeric data (time-averaged for charts)
    IEnumerable<NumericDto> GlucoseLevels,
    IEnumerable<NumericDto> HeartRates,
    // Full DTOs with all properties
    IEnumerable<NumericDto> SleepSessions,
    IEnumerable<WorkoutDto> Workouts,
    IEnumerable<ManualInputDto> Medications,
    ManualInputDto? Menstruation
);