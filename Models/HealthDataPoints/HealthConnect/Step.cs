namespace Diabits.API.Models.HealthDataPoints.HealthConnect;

/// <summary>
/// Concrete HealthDataPoint representing step count data.
/// </summary>

public class Step : HealthDataPoint
{
    // Number of steps recorded for the time window (StartTime..EndTime).
    public int Count { get; set; }
}
