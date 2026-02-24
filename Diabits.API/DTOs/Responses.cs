using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.ManualInput;

namespace Diabits.API.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken);

public record ManualInputResponse(Menstruation? Menstruation, List<Medication?> Medications);

public record HealthDataResponse(
    IEnumerable<NumericDto> GlucoseLevels,
    IEnumerable<NumericDto> HeartRates,
    IEnumerable<NumericDto> Steps,
    IEnumerable<NumericDto> SleepSessions,
    IEnumerable<WorkoutDto> Workouts,
    IEnumerable<ManualInputDto> Medications,
    IEnumerable<ManualInputDto> Menstruation
);

public record LastSyncResponse(DateTime? LastSyncAt);

public record InviteResponse(string Email, string Code, DateTime CreatedAt, DateTime? UsedAt, string? UsedBy);