namespace Diabits.API.DTOs.HealthDataPoints;

public class ImportDto : HealthDataPointBaseDto
{
    public double Units { get; set; }
    public double? CarbGrams { get; set; }
    public double? GlucoseLevel { get; set; }
}