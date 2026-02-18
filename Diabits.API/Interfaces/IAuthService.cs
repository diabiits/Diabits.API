using Diabits.API.DTOs;

namespace Diabits.API.Interfaces;

public interface IAuthService   
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshAccessTokenAsync(string userId, string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<string> UpdateCredentialsAsync(string userId, UpdateCredentialsRequest request);
}