using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Diabits.API.Models.HealthDataPoints.ManualInput;

namespace Diabits.API.Data;

/// <summary>
/// Static extension methods that map incoming DTOs to domain entity types. 
/// </summary>
public static class HealthDataMapper
{
    // Map incoming numeric DTOs (Health Connect numerics) to concrete HealthDataPoint entities.
    // Returns null for types that are not supported by the server.
    public static HealthDataPoint? MapToEntity(this NumericDto dto)
    {
        var type = dto.HealthDataType.Trim().ToUpperInvariant();

        return type switch
        {
            "BLOOD_GLUCOSE" => MapGlucoseLevel(dto),
            "HEART_RATE" => MapHeartRate(dto),
            "SLEEP_SESSION" => MapSleepSession(dto),
            "STEPS" => MapStep(dto),
            _ => null // Unknown numeric type -> skip
        };
    }

    // Map manual input DTOs (sent from client UI) to domain entities (Menstruation, Medication).
    // Returns null when the manual input type is not recognized.
    public static HealthDataPoint? MapToEntity(this ManualInputDto dto)
    {
        var type = dto.HealthDataType.Trim().ToUpperInvariant();

        return type switch
        {
            "MENSTRUATION" => MapMenstruation(dto),
            "MEDICATION" => MapMedication(dto),
            _ => null // Unknown manual type -> skip
        };
    }

    // Map workout DTO to Workout entity.
    public static Workout MapToEntity(this WorkoutDto dto) => new Workout
    {
        StartTime = dto.DateFrom,
        EndTime = dto.DateTo,
        Type = HealthDataType.WORKOUT,
        ActivityType = dto.HealthValue!.ActivityType,
        // CaloriesBurned may be null; convert safely to int (0 when null).
        CaloriesBurned = Convert.ToInt32(dto.HealthValue?.CaloriesBurned ?? 0)
    };

    //TODO Check if coming in in mmol/l now
    // Numeric -> Glucose entity.
    // Note: client numeric value is converted from mg/dL to mmol/L using factor 18.0182.
    private static GlucoseLevel MapGlucoseLevel(NumericDto dto) => new GlucoseLevel
    {
        StartTime = dto.DateFrom,
        Type = HealthDataType.BLOOD_GLUCOSE,
        mmolL = dto.HealthValue!.NumericValue  /// 18.0182
    };

    // Numeric -> HeartRate entity (BPM).
    private static HeartRate MapHeartRate(NumericDto dto) => new HeartRate
    {
        StartTime = dto.DateFrom,
        Type = HealthDataType.HEART_RATE,
        BPM = Convert.ToInt32(dto.HealthValue!.NumericValue)
    };

    // Numeric -> SleepSession entity (duration in minutes).
    private static SleepSession MapSleepSession(NumericDto dto) => new SleepSession
    {
        StartTime = dto.DateFrom,
        EndTime = dto.DateTo,
        Type = HealthDataType.SLEEP_SESSION,
        DurationMinutes = Convert.ToInt32(dto.HealthValue!.NumericValue)
    };

    // Numeric -> Step entity (count).
    private static Step MapStep(NumericDto dto) => new Step
    {
        StartTime = dto.DateFrom,
        EndTime = dto.DateTo,
        Type = HealthDataType.STEPS,
        Count = Convert.ToInt32(dto.HealthValue!.NumericValue)
    };

    // Manual input -> Menstruation entity.
    // Parses Flow enum case-insensitively; dto.Flow must be present.
    private static Menstruation MapMenstruation(ManualInputDto dto) => new Menstruation
    {
        StartTime = dto.DateFrom,
        Type = HealthDataType.MENSTRUATION,
        Flow = Enum.Parse<FlowEnum>(dto.Flow!, true) // ignoreCase = true
    };

    // Manual input -> Medication entity.
    // Copies medication name and amount from DTO to entity.
    private static Medication MapMedication(ManualInputDto dto) => new Medication
    {
        StartTime = dto.DateFrom,
        Type = HealthDataType.MEDICATION,
        Name = dto.Medication!.Name,
        Quantity = dto.Medication!.Quantity,
        StrengthValue = dto.Medication!.StrengthValue,
        StrengthUnit = Enum.Parse<StrengthUnit>(dto.Medication!.StrengthUnit, true),
    };
}