namespace Diabits.API.Models.HealthDataPoints.HealthConnect;

/// <summary>
/// Concrete HealthDataPoint representing a heart rate measurement (beats per minute).
/// </summary>
public class HeartRate : HealthDataPoint
{
    // Beats per minute measured at StartTime.
    public int BPM { get; set; }
}
