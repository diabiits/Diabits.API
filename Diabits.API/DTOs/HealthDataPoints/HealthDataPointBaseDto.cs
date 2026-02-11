using System.Text.Json.Serialization;

namespace Diabits.API.DTOs.HealthDataPoints;

// Base DTO for health-data payloads exchanged with the client. 
public abstract class HealthDataPointBaseDto
{
    // Optional server-side id. Present for entities previously persisted; null for new items.
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    //TODO JsonConverter for polymorphic deserialization based on this type discriminator value, to avoid manual mapping in the service layer?
    // String representation of the health data type. Used by mapping logic to decide which concrete entity to create.
    [JsonPropertyName("type")]
    public string HealthDataType { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("dateFrom")]
    public DateTime DateFrom { get; set; }

    // Optional end timestamp for ranged data (sleep sessions, workouts). Null for instant values.
    [JsonPropertyName("dateTo")]
    public DateTime? DateTo { get; set; }
}