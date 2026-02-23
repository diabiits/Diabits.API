namespace Diabits.API.Models.HealthDataPoints.ManualInput;

/// <summary>
/// Concrete HealthDataPoint representing a manual medication entry created by the user.
/// </summary>

public class Medication : HealthDataPoint
{
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double StrengthValue { get; set; }
    public StrengthUnit StrengthUnit { get; set; }

}

public enum StrengthUnit
{
    Mg,
    Mcg,
    G,
    Ml,
    IU,
}