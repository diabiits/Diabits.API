namespace Diabits.API.Models.HealthDataPoints.HealthConnect;

// TODO Consider adding more detailed sleep data (e.g., sleep stages)
/// <summary>
/// Concrete HealthDataPoint representing a sleep session (duration in minutes).
/// </summary>
public class SleepSession : HealthDataPoint
{
    // StartTime/EndTime capture the time window; DurationMinutes is an explicit numeric summary for quick queries.
    public int DurationMinutes { get; set; }
}
