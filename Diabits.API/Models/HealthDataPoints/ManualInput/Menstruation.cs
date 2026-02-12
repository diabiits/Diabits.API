using System.Text.Json.Serialization;

namespace Diabits.API.Models.HealthDataPoints.ManualInput;

/// <summary>
/// Concrete HealthDataPoint representing a manual menstruation entry created by the user.
/// </summary>
public class Menstruation : HealthDataPoint
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FlowEnum Flow { get; set; }
}

public enum FlowEnum
{
    SPOTTING,
    LIGHT,
    MEDIUM,
    HEAVY
}