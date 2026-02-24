using Diabits.API.Data;
using Diabits.API.Interfaces;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Diabits.API.Models.HealthDataPoints.ManualInput;

using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Services;

public class TimelineDashboardService : ITimelineDashboardService
{
    private readonly DiabitsDbContext _dbContext;

    public TimelineDashboardService(DiabitsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private const int BucketMinutes = 10;
    private const int BucketsPerDay = 1440 / BucketMinutes;

    public async Task<TimelineChartResponse> GetTimelineAsync(string userId, DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1).AddMilliseconds(-1);

        var glucoseLevels = await QueryDay<GlucoseLevel>(userId, dayStart, dayEnd)
            .Select(x => new NumericPoint(x.StartTime, (double)x.mmolL))
            .ToListAsync();

        var heartRates = await QueryDay<HeartRate>(userId, dayStart, dayEnd)
            .Select(x => new NumericPoint(x.StartTime, (double)x.BPM))
            .ToListAsync();

        var sleeps = await QueryDay<SleepSession>(userId, dayStart, dayEnd)
            .Select(x => new Interval
            (
                x.StartTime,
                x.EndTime!.Value,
                FormatDurationMinutes(x.DurationMinutes)
            ))
            .ToListAsync();

        var workouts = await QueryDay<Workout>(userId, dayStart, dayEnd)
            .Select(x => new Interval
            (
                x.StartTime,
                x.EndTime!.Value,
                FormatWorkoutLabel(x.ActivityType, x.CaloriesBurned, x.StartTime, x.EndTime!.Value)
            ))
            .ToListAsync();

        var meds = await QueryDay<Medication>(userId, dayStart, dayEnd)
            .Select(x => new Marker(
                x.StartTime,
                FormatMedicationLabel(x.Name, x.Quantity, x.StrengthValue, x.StrengthUnit)
            ))
            .ToListAsync();

        var flow = await QueryDay<Menstruation>(userId, dayStart, dayEnd)
            .Select(x => (FlowEnum?)x.Flow)
            .FirstOrDefaultAsync();

        var series = new List<TimelineSeries>
        {
            new(
                Name: "Glucose",
                Type: TimelineSeriesType.Line,
                Points: BucketNumeric(glucoseLevels, dayStart, "mmol/L")
            ),
            new(
                Name: "Heart Rate",
                Type: TimelineSeriesType.Line,
                Points: BucketNumeric(heartRates, dayStart, "BPM", decimals: 0)
            ),
            new(
                Name: "Sleep",
                Type: TimelineSeriesType.Area,
                Points: BucketBooleanIntervals(sleeps, dayStart)
            ),
            new(
                Name: "Workout",
                Type: TimelineSeriesType.Area,
                Points: BucketBooleanIntervals(workouts, dayStart)
            ),
            new(
                Name: "Menstruation",
                Type: TimelineSeriesType.Area,
                Points: BucketMenstruation(flow, dayStart)
            ),
            new(
                Name: "Medication",
                Type: TimelineSeriesType.Scatter,
                Points: BucketMarkers(meds, dayStart)
            ),
        };

        return new TimelineChartResponse(series);
    }


    private static List<TimelinePoint> BucketNumeric(IEnumerable<NumericPoint> points, DateTime dayStart, string unit, int decimals = 1)
    {
        var buckets = CreateBuckets(dayStart);

        var byBucket = points
            .GroupBy(p => BucketIndex(p.Time, dayStart))
            .Where(g => g.Key >= 0 && g.Key < BucketsPerDay)
            .ToDictionary(
                g => g.Key,
                g => Math.Round(g.Average(x => x.Value), decimals)
            );

        var fmt = $"F{decimals}";

        foreach (var (idx, value) in byBucket)
        {
            buckets[idx] = buckets[idx] with
            {
                Value = value,
                Name = $"{value.ToString(fmt)} {unit}"
            };
        }

        return buckets;
    }

    private static List<TimelinePoint> BucketBooleanIntervals(IEnumerable<Interval> intervals, DateTime dayStart, double activeValue = 1)
    {
        var buckets = CreateBuckets(dayStart);


        foreach (var interval in intervals)
        {
            var startIdx = Math.Clamp(BucketIndex(interval.Start, dayStart), 0, BucketsPerDay - 1);
            var endIdx = Math.Clamp(BucketIndex(interval.End, dayStart), 0, BucketsPerDay - 1);

            for (var i = startIdx; i <= endIdx; i++)
            {
                buckets[i] = buckets[i] with
                {
                    Value = activeValue,
                    Name = interval.Label
                };
            }
        }

        for (var i = 0; i < buckets.Count; i++)
            buckets[i] = buckets[i] with { Value = buckets[i].Value ?? 0 };

        return buckets;
    }


    private static List<TimelinePoint> BucketMarkers(IEnumerable<Marker> markers, DateTime dayStart, double markerValue = 0.5)
    {
        var buckets = CreateBuckets(dayStart);

        var namesPerBucket = new List<string>[BucketsPerDay];
        for (var i = 0; i < BucketsPerDay; i++) namesPerBucket[i] = [];

        foreach (var m in markers)
        {
            var idx = BucketIndex(m.Time, dayStart);
            if (idx < 0 || idx >= BucketsPerDay) continue;
            namesPerBucket[idx].Add(m.Label);
        }

        for (var i = 0; i < BucketsPerDay; i++)
        {
            if (namesPerBucket[i].Count == 0) continue;

            var label = string.Join(", ", namesPerBucket[i].Distinct());
            buckets[i] = buckets[i] with { Value = markerValue, Name = label };
        }

        return buckets;
    }

    private static List<TimelinePoint> BucketMenstruation(FlowEnum? flow, DateTime dayStart)
    {
        var buckets = CreateBuckets(dayStart);

        var (value, label) = flow switch
        {
            FlowEnum.SPOTTING => (0.1, "Spotting"),
            FlowEnum.LIGHT => (0.25, "Light"),
            FlowEnum.MEDIUM => (0.5, "Medium"),
            FlowEnum.HEAVY => (0.75, "Heavy"),
            _ => (0.0, (string?)null)
        };

        for (var i = 0; i < buckets.Count; i++)
        {
            buckets[i] = buckets[i] with
            {
                Value = value,
                Name = label
            };
        }

        return buckets;
    }

    private IQueryable<T> QueryDay<T>(string userId, DateTime dayStart, DateTime dayEnd) where T : HealthDataPoint
        => _dbContext.Set<T>().AsNoTracking().Where(x => x.UserId == userId && x.StartTime >= dayStart && x.StartTime < dayEnd);

    private static List<TimelinePoint> CreateBuckets(DateTime dayStart)
    {
        var result = new List<TimelinePoint>(BucketsPerDay);

        for (var i = 0; i < BucketsPerDay; i++)
        {
            result.Add(new TimelinePoint(
                Time: dayStart.AddMinutes(i * BucketMinutes),
                Value: null,
                Name: null
            ));
        }
        return result;
    }

    private static int BucketIndex(DateTime time, DateTime dayStart) => (int)((time - dayStart).TotalMinutes / BucketMinutes);

    private static string FormatDurationMinutes(int minutes)
    {
        var h = minutes / 60;
        var m = minutes % 60;
        return h > 0 ? $"{h}h {m}m" : $"{m}m";
    }
    private static string FormatWorkoutLabel(string activityType, int caloriesBurned, DateTime start, DateTime end)
    {
        var activity = string.IsNullOrWhiteSpace(activityType) ? "Workout" : activityType.Trim();

        var minutes = (int)Math.Max(0, (end - start).TotalMinutes);
        var duration = FormatDurationMinutes(minutes);

        var parts = new List<string> { activity };

        if (caloriesBurned > 0) parts.Add($"{caloriesBurned} kcal");
        if (!string.IsNullOrWhiteSpace(duration)) parts.Add(duration);

        return string.Join(" · ", parts);
    }

    private static string FormatMedicationLabel(string name, double quantity, double strengthValue, StrengthUnit strengthUnit) => $"{name} {strengthValue * quantity}{strengthUnit.ToString().ToLowerInvariant()}";

    private readonly record struct NumericPoint(DateTime Time, double Value);
    private readonly record struct Marker(DateTime Time, string Label);
    private readonly record struct Interval(DateTime Start, DateTime End, string Label);
}

public record TimelineChartResponse(List<TimelineSeries> Series);

public record TimelineSeries(string Name, TimelineSeriesType Type, List<TimelinePoint> Points);

public record TimelinePoint(DateTime Time, double? Value, string? Name = null);

public enum TimelineSeriesType
{
    Line,
    Area,
    Scatter
}