using System.Text.Json.Serialization;

namespace Diabits.API.DTOs.HealthDataPoints;

/// <summary>
/// DTO for workout data exchanged between client and server. 
/// </summary>
public class WorkoutDto : HealthDataPointBaseDto
{
    [JsonPropertyName("value")]
    public WorkoutHealthValueDto HealthValue { get; set; } = new();
}

public class WorkoutHealthValueDto
{
    [JsonPropertyName("workoutActivityType")]
    public string ActivityType { get; set; } = string.Empty;

    [JsonPropertyName("totalEnergyBurned")]
    public double? CaloriesBurned { get; set; }
}