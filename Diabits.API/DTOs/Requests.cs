using Diabits.API.DTOs.HealthDataPoints;

namespace Diabits.API.DTOs;

// Simple request DTOs (records) representing incoming API payloads.
public record RegisterRequest(string Username, string Password, string Email, string InviteCode);
public record LoginRequest(string Username, string Password);
public record RefreshTokenRequest(string RefreshToken);

public record CreateInviteRequest(string Email);

public record HealthConnectRequest(IEnumerable<NumericDto> Numerics, IEnumerable<WorkoutDto> Workouts);
public record ManualInputRequest(IEnumerable<ManualInputDto> Items);
public record BatchDeleteManualInputRequest(IEnumerable<int> Ids);

public record LastSyncRequest(DateTime SyncTime);