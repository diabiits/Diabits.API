namespace Diabits.API.DTOs;

// Simple request DTOs (records) representing incoming API payloads.
public record RegisterRequest(string Username, string Password, string Email, string InviteCode);
public record LoginRequest(string Username, string Password);
public record RefreshTokenRequest(string RefreshToken);