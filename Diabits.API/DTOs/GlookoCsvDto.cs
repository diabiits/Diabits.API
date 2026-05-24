namespace Diabits.API.DTOs;

public class GlookoCsvDto
{
    public DateTime Timestamp { get; set; }
    public double? GlucoseLevel { get; set; }
    public int? Carbs { get; set; }
    public double? Insulin { get; set; }
}