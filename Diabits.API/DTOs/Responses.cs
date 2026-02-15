using Diabits.API.Models.HealthDataPoints.ManualInput;

namespace Diabits.API.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken);

public record ManualInputResponse(Menstruation? Menstruation, List<Medication?> Medications);

public record LastSyncResponse(DateTime? LastSyncAt);

public record InviteResponse(string Email, string Code, DateTime CreatedAt, DateTime? UsedAt, string? UsedBy);
