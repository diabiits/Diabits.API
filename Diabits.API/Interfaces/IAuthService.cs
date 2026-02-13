using Diabits.API.DTOs;
using Diabits.API.Models;

namespace Diabits.API.Interfaces;

public interface IAuthService   
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshAccessTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);

    //TODO Remove after cred update done
    Task<string> GenerateAccessTokenAsync(DiabitsUser user);


}