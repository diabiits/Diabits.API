using Diabits.API.Models.HealthDataPoints;
using System.Text.Json.Serialization;

namespace Diabits.API.DTOs.HealthDataPoints;

// Base DTO for health-data payloads exchanged with the client. 
public abstract class HealthDataPointBaseDto
{
    // Optional server-side id. Present for entities previously persisted; null for new items.
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("type")]
    public HealthDataType HealthDataType { get; set; }

    //TODO Ensure that not used and then delete
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("dateFrom")]
    public DateTime DateFrom { get; set; }

    // Optional end timestamp for ranged data (sleep sessions, workouts). Null for instant values.
    [JsonPropertyName("dateTo")]
    public DateTime? DateTo { get; set; }
}