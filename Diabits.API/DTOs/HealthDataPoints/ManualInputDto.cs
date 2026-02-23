using Diabits.API.Models.HealthDataPoints.ManualInput;
using System.Text.Json.Serialization;

namespace Diabits.API.DTOs.HealthDataPoints;

/// <summary>
/// DTO used for client -> server manual input payloads (medication entries and menstruation entries).
/// </summary>
public class ManualInputDto : HealthDataPointBaseDto
{
    // When the DTO represents medication input this object contains name/amount
    [JsonPropertyName("medication")]
    public MedicationValueDto? Medication { get; set; }

    // When the DTO represents a menstruation entry this string holds the flow category (e.g., "LIGHT")
    [JsonPropertyName("flow")]
    public FlowEnum? Flow { get; set; }
}

public class MedicationValueDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("strengthValue")]
    public decimal StrengthValue { get; set; }

    [JsonPropertyName("strengthUnit")]
    public string StrengthUnit { get; set; } = string.Empty;
}
