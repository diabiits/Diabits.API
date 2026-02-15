using System.Text.Json.Serialization;

namespace Diabits.API.DTOs.HealthDataPoints;

/// <summary>
/// DTO for numeric health measurements (heart rate, glucose, steps, sleep duration, etc.). 
/// </summary>
public class NumericDto : HealthDataPointBaseDto
{
    [JsonPropertyName("value")]
    public NumericHealthValueDto HealthValue { get; set; } = null!;
}

public class NumericHealthValueDto
{
    // The numeric measurement value (e.g., mmol/L for glucose, bpm for heart rate, count for steps).
    [JsonPropertyName("numericValue")]
    public double NumericValue { get; set; }
}