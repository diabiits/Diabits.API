using Diabits.API.Data;
using Diabits.API.Interfaces;
using Diabits.API.Models.HealthDataPoints.HealthConnect;

using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Services;

public class GlucoseDashboardService : IGlucoseDashboardService
{
    private readonly DiabitsDbContext _dbContext;

    public GlucoseDashboardService(DiabitsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DailyGlucoseResponse> GetDailyGlucoseAsync(string userId, DateOnly date)
    {
        var selectedStart = date.ToDateTime(TimeOnly.MinValue);
        var selectedEnd = selectedStart.AddDays(1);

        const int daysBack = 7;
        const int bucketMinutes = 10;
        var bucketCount = (int)TimeSpan.FromDays(1).TotalMinutes / bucketMinutes;

        var windowStart = selectedStart.AddDays(-daysBack);

        // Pull once for the whole window: [windowStart, selectedEnd)
        var allReadings = await _dbContext
            .Set<GlucoseLevel>()
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.StartTime >= windowStart && x.StartTime < selectedEnd)
            .OrderBy(x => x.StartTime)
            .Select(x => new Reading(x.StartTime, (double)x.mmolL))
            .ToListAsync();

        // Selected day raw readings (for stats + percentages)
        var readingsForDay = allReadings
            .Where(x => x.StartTime >= selectedStart && x.StartTime < selectedEnd)
            .Select(x => new DailyGlucoseReading(x.StartTime, x.Value))
            .ToList();

        // Always return aligned weekly range and aligned bucket timeline
        var buckets = BuildDayBuckets(selectedStart, bucketMinutes, bucketCount, readingsForDay);
        var weeklyRange = BuildWeeklyRange(allReadings, selectedStart, daysBack, bucketMinutes, bucketCount);

        if (readingsForDay.Count == 0)
        {
            return new DailyGlucoseResponse(
                Readings: [],
                Buckets: buckets,
                Stats: new DailyGlucoseStats(0, 0, 0, 0),
                Ranges: new DailyGlucoseRanges(0, 0, 0, 0, 0),
                WeeklyRange: weeklyRange
            );
        }

        var values = readingsForDay.Select(r => r.Value).ToList();
        var count = values.Count;

        var stats = new DailyGlucoseStats(
            Average: values.Average(),
            Min: values.Min(),
            Max: values.Max(),
            Count: count
        );

        int VeryLowCount() => values.Count(v => v <= 3.0);
        int LowCount() => values.Count(v => v >= 3.1 && v <= 3.9);
        int InRangeCount() => values.Count(v => v >= 4.0 && v <= 9.9);
        int HighCount() => values.Count(v => v >= 10.0 && v <= 13.8);
        int VeryHighCount() => values.Count(v => v >= 13.9);
        double P(int c) => (c / (double)count) * 100.0;

        var ranges = new DailyGlucoseRanges(
            VeryLow: P(VeryLowCount()),
            Low: P(LowCount()),
            InRange: P(InRangeCount()),
            High: P(HighCount()),
            VeryHigh: P(VeryHighCount())
        );

        return new DailyGlucoseResponse(
            Readings: readingsForDay,
            Buckets: buckets,
            Stats: stats,
            Ranges: ranges,
            WeeklyRange: weeklyRange
        );
    }

    private static List<DailyBucketPoint> BuildDayBuckets(
        DateTime selectedStart,
        int bucketMinutes,
        int bucketCount,
        List<DailyGlucoseReading> readingsForDay
    )
    {
        var result = new List<DailyBucketPoint>(bucketCount);
        var ordered = readingsForDay.OrderBy(r => r.Time).ToList();

        for (int i = 0; i < bucketCount; i++)
        {
            var bucketStart = selectedStart.AddMinutes(i * bucketMinutes);
            var bucketEnd = bucketStart.AddMinutes(bucketMinutes);

            var vals = ordered
                .Where(r => r.Time >= bucketStart && r.Time < bucketEnd)
                .Select(r => r.Value)
                .ToList();

            double? avg = vals.Count > 0 ? Math.Round(vals.Average(), 1) : null;
            result.Add(new DailyBucketPoint(bucketStart, avg));
        }

        return result;
    }

    private static List<DailyRangePoint> BuildWeeklyRange(
        List<Reading> allReadings,
        DateTime selectedStart,
        int daysBack,
        int bucketMinutes,
        int bucketCount
    )
    {
        var weeklyRange = new List<DailyRangePoint>(bucketCount);

        for (int i = 0; i < bucketCount; i++)
        {
            var bucketStart = selectedStart.AddMinutes(i * bucketMinutes);
            var valuesInBucket = new List<double>();

            for (int d = 1; d <= daysBack; d++)
            {
                var dayToCheckStart = bucketStart.Date.AddDays(-d).Add(bucketStart.TimeOfDay);
                var dayToCheckEnd = dayToCheckStart.AddMinutes(bucketMinutes);

                var matches = allReadings
                    .Where(r => r.StartTime >= dayToCheckStart && r.StartTime < dayToCheckEnd)
                    .Select(r => r.Value);

                if (matches.Any())
                    valuesInBucket.AddRange(matches);
            }

            double? minVal = null, maxVal = null;
            if (valuesInBucket.Count > 0)
            {
                minVal = Math.Round(valuesInBucket.Min(), 1);
                maxVal = Math.Round(valuesInBucket.Max(), 1);
            }

            weeklyRange.Add(new DailyRangePoint(bucketStart, minVal, maxVal));
        }

        return weeklyRange;
    }

    private record Reading(DateTime StartTime, double Value);

    public record DailyGlucoseResponse(
        List<DailyGlucoseReading> Readings,
        List<DailyBucketPoint> Buckets,
        DailyGlucoseStats Stats,
        DailyGlucoseRanges Ranges,
        List<DailyRangePoint> WeeklyRange
    );

    public record DailyGlucoseReading(DateTime Time, double Value);
    public record DailyBucketPoint(DateTime Time, double? Value);
    public record DailyGlucoseStats(double Average, double Min, double Max, int Count);
    public record DailyGlucoseRanges(double VeryLow, double Low, double InRange, double High, double VeryHigh);
    public record DailyRangePoint(DateTime Time, double? Min, double? Max);
}