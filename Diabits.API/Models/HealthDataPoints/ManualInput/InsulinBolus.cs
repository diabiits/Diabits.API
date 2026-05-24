namespace Diabits.API.Models.HealthDataPoints.ManualInput;

public class InsulinBolus : HealthDataPoint
{
    public double Units { get; set; }
    public double? CarbGrams { get; set; }
    public double? GlucoseLevel { get; set; }
}
