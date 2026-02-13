using Diabits.API.Models.HealthDataPoints.ManualInput;

namespace Diabits.API.DTOs;

// Authentication response returned after login/refresh/register flows.
// Carries an access token, its expiration instant, and the long-lived refresh token.
public record AuthResponse(string AccessToken, string RefreshToken);

public record ManualInputResponse(Menstruation? Menstruation, List<Medication?> Medications);

public record LastSyncResponse(DateTime? LastSyncAt);