using System.Text.Json.Serialization;

namespace Diabits.API.Models.HealthDataPoints;

// TODO Consider making DateTimes offset, if there's time and effort is worth it
/// <summary>
/// Base class for shared fields used by all health data point concrete types.
/// </summary>
public abstract class HealthDataPoint
{
    public int Id { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HealthDataType Type { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? UserId { get; set; }
    public DiabitsUser User { get; set; } = null!;
}

public enum HealthDataType
{
    BLOOD_GLUCOSE,
    STEPS,
    HEART_RATE,
    SLEEP_SESSION,
    WORKOUT,
    MENSTRUATION,
    MEDICATION
}