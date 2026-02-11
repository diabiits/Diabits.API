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
    //TODO Rethink?
    // Client-provided type discriminator.
    // Used by the client to indicate subtype information; server mapping relies primarily on HealthDataType.
    [JsonPropertyName("__type")]
    public string TypeIndicator { get; set; } = string.Empty;

    // The numeric measurement value (e.g., mmol/L for glucose, bpm for heart rate, count for steps).
    [JsonPropertyName("numericValue")]
    public double NumericValue { get; set; }
}