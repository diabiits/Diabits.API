namespace Diabits.API.Models.HealthDataPoints.HealthConnect;

/// <summary>
/// Concrete HealthDataPoint representing a workout activity recorded by the client
/// </summary>
public class Workout : HealthDataPoint
{
    // Human-readable activity type provided by the client (e.g., Running, Walking)
    public string ActivityType { get; set; } = null!;

    // Total calories burned during the workout. StartTime/EndTime capture the time window
    public int CaloriesBurned { get; set; }
}
