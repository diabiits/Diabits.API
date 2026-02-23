namespace Diabits.API.Models.HealthDataPoints.HealthConnect;

/// <summary>
/// Concrete HealthDataPoint for blood glucose measurements stored in mmol/L (European standard)
/// </summary>
public class GlucoseLevel : HealthDataPoint
{
    public double mmolL { get; set; }
}
