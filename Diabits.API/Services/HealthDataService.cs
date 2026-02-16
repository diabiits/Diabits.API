using Diabits.API.Data;
using Diabits.API.DTOs;
using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Helpers;
using Diabits.API.Helpers.Mapping;
using Diabits.API.Interfaces;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Diabits.API.Models.HealthDataPoints.ManualInput;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Services;

public class HealthDataService(DiabitsDbContext dbContext, MapperFactory mapperFactory) : IHealthDataService
{
    private readonly DiabitsDbContext _dbContext = dbContext;
    private readonly MapperFactory _mapperFactory = mapperFactory;

    /// <summary>
    /// Returns manual input entries for a single day grouped into menstruation (single entry) and medication (list).
    /// </summary>
    public async Task<ManualInputResponse> GetManualInputForDayAsync(string userId, DateTime date)
    {
        var startOfDay = date.AtMidnight();
        var endOfDay = startOfDay.AddDays(1); // Inclusive end of day


        var meds = await _dbContext
             .Set<Medication>()
             .AsNoTracking()
             .Where(m => m.UserId == userId && m.StartTime >= startOfDay && m.StartTime <= endOfDay)
             .ToListAsync();

        var mens = await _dbContext
            .Set<Menstruation>()
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.StartTime >= startOfDay && m.StartTime <= endOfDay)
            .FirstOrDefaultAsync();

        return new ManualInputResponse(
            Menstruation: mens,
            Medications: [.. meds.Cast<Medication?>()]
            );
    }

    /// <summary>
    /// Returns all health data separated by concrete type for a user within a specified time period.
    /// Queries each type separately for better performance with large datasets.
    /// </summary>
    public async Task<HealthDataResponse> GetHealthDataForPeriodAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var start = startDate.AtMidnight();
        var end = endDate.AtMidnight().AddDays(1);

        // Query each type separately to avoid loading everything into memory at once
        var glucoseLevels = await _dbContext.Set<GlucoseLevel>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var heartRates = await _dbContext.Set<HeartRate>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var steps = await _dbContext.Set<Step>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var sleepSessions = await _dbContext.Set<SleepSession>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var workouts = await _dbContext.Set<Workout>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var medications = await _dbContext.Set<Medication>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        var menstruations = await _dbContext.Set<Menstruation>()
            .AsNoTracking()
            .Where(dp => dp.UserId == userId && dp.StartTime >= start && dp.StartTime <= end)
            .ToListAsync();

        return new HealthDataResponse(
            GlucoseLevels: [.. glucoseLevels.Select(e => (NumericDto)_mapperFactory.ToDto(e))],
            HeartRates: [.. heartRates.Select(e => (NumericDto)_mapperFactory.ToDto(e))],
            Steps: [.. steps.Select(e => (NumericDto)_mapperFactory.ToDto(e))],
            SleepSessions: [.. sleepSessions.Select(e => (NumericDto)_mapperFactory.ToDto(e))],
            Workouts: [.. workouts.Select(e => (WorkoutDto)_mapperFactory.ToDto(e))],
            Medications: [.. medications.Select(e => (ManualInputDto)_mapperFactory.ToDto(e))],
            Menstruation: [.. menstruations.Select(e => (ManualInputDto)_mapperFactory.ToDto(e))]
        );
    }

    /// <summary>
    ///  Convert incoming DTOs (workouts + numerics) into mapped entity instances and persist them.
    ///  The mobile client already performs some deduplication; server still performs a safety dedupe.
    /// </summary>
    public async Task AddDataPointsAsync(HealthConnectRequest request, string userId)
    {
        // Ensure user exists and get a reference for updating last sync.
        var user = await _dbContext.Users.SingleAsync(u => u.Id == userId);

        var dataPoints = new List<HealthDataPoint>();

        foreach (var w in request.Workouts)
        {
            var dataPoint = _mapperFactory.FromDto(w);
            dataPoints.Add(dataPoint);
        }

        foreach (var n in request.Numerics)
        {
            var dataPoint = _mapperFactory.FromDto(n);
            dataPoints.Add(dataPoint);
        }

        await AddDataPointsAsync(dataPoints, user.Id);
    }


    /// <summary>
    ///  Convert incoming manual input DTOs (menstruation + medications) into mapped entity instances and persist.
    /// </summary>
    public async Task AddDataPointsAsync(ManualInputRequest request, string userId)
    {
        var user = await _dbContext.Users.SingleAsync(u => u.Id == userId);

        var dataPoints = new List<HealthDataPoint>();

        foreach (var i in request.Items)
        {
            var dataPoint = _mapperFactory.FromDto(i);
            dataPoints.Add(dataPoint);
        }

        await AddDataPointsAsync(dataPoints, user.Id);
    }

    /// <summary>
    /// Mapped datapoints to DB (with deduplication against existing data)
    /// </summary>
    private async Task AddDataPointsAsync(List<HealthDataPoint> points, string userId)
    {
        if (points.Count == 0) return;

        var start = points.Min(p => p.StartTime);
        var end = points.Max(p => p.EndTime);

        // Load existing datapoints from the computed window for this user
        var existingPoints = await _dbContext.HealthDataPoints
            .Where(dp => dp.UserId == userId && dp.StartTime >= start)
            .ToListAsync();

        var deduplicationStrategy = new DeduplicationStrategy(existingPoints);

        var toAdd = points
            .Where(p => !deduplicationStrategy.IsDuplicate(p))
            .ToList();

        if (toAdd.Count == 0) return;

        // Assign ownership before persistence
        foreach (var dp in toAdd)
            dp.UserId = userId;

        await _dbContext.HealthDataPoints.AddRangeAsync(toAdd);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Update multiple manual input entries in a batch for the user.
    /// </summary>
    public async Task<int> BatchUpdateManualInputAsync(string userId, IEnumerable<ManualInputDto> inputDtos)
    {
        var dtos = inputDtos as IList<ManualInputDto> ?? [.. inputDtos];
        if (dtos.Count == 0) return 0;

        // Extract all IDs from the DTOs (filter out null IDs)
        var idSet = dtos
            .Select(dto => dto.Id)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        if (idSet.Count == 0) return 0;

        var medications = await _dbContext.Set<Medication>()
            .Where(m => m.UserId == userId && idSet.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        var menstruations = await _dbContext.Set<Menstruation>()
            .Where(m => m.UserId == userId && idSet.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);


        var updateCount = 0;

        foreach (var dto in dtos)
        {
            if (!dto.Id.HasValue) continue;
            var id = dto.Id.Value;

            if (dto.Medication is not null && medications.TryGetValue(dto.Id.Value, out var med))
            {
                _mapperFactory.UpdateManualInput(dto, med);
                updateCount++;
            }
            else if (menstruations.TryGetValue(id, out var mens))
            {
                _mapperFactory.UpdateManualInput(dto, mens);
                updateCount++;
            }
        }

        // Persist all changes in a single transaction
        if (updateCount > 0)
            await _dbContext.SaveChangesAsync();

        return updateCount;
    }

    /// <summary>
    /// Delete multiple manual input entries in a batch belonging to the user.
    /// </summary>
    public async Task<int> BatchDeleteManualInputAsync(string userId, IEnumerable<int> ids)
    {
        var idSet = ids.ToHashSet();
        if (idSet.Count == 0) return 0;


        var deletedMedications = await _dbContext.Set<Medication>()
            .Where(m => m.UserId == userId && idSet.Contains(m.Id))
            .ExecuteDeleteAsync();

        var deletedMenstruations = await _dbContext.Set<Menstruation>()
            .Where(m => m.UserId == userId && idSet.Contains(m.Id))
            .ExecuteDeleteAsync();

        return deletedMedications + deletedMenstruations;
    }
}