using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Diabits.API.Data;
using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Diabits.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Diabits.API.Services;

public class AuthService(UserManager<DiabitsUser> userManager, DiabitsDbContext dbContext, IConfiguration config) : IAuthService
{
    private readonly UserManager<DiabitsUser> _userManager = userManager;
    private readonly DiabitsDbContext _dbContext = dbContext;
    private readonly IConfiguration _config = config;

    /// <summary>
    /// Validate credentials and return AuthResponse containing access token, expiration and refresh token
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username) ?? throw new InvalidOperationException("Invalid credentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            throw new InvalidOperationException("Invalid credentials");

        // Create access- and refresh token on successful authentication
        var (accessToken, expiration) = await GenerateAccessTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);

        return new AuthResponse(accessToken, expiration, refreshToken);
    }

    /// <summary>
    /// Delete a refresh token on logout. If token not found, operation is idempotent
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

        if (stored == null)
            return;

        // Delete the refresh token so it cannot be used again.
        _dbContext.RefreshTokens.Remove(stored);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Register a new user using an invite code
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Invite-based registration: ensure invite exists, unused and email matches
        var invite = await _dbContext.Invites.FirstOrDefaultAsync(i => i.Code == request.InviteCode && i.UsedAt == null);
        
        if (invite == null || invite.Email != request.Email)
            throw new InvalidOperationException("Invalid invite");

        // Prevent username collisions
        var existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
            throw new InvalidOperationException("Username already taken");

        var newUser = new DiabitsUser()
        {
            UserName = request.Username,
            Email = request.Email,
            InviteId = invite.Id
        };

        var userCreation = await _userManager.CreateAsync(newUser, request.Password);
        if (!userCreation.Succeeded)
            throw new Exception("User creation failed! Please check user details and try again.");

        // Assign the default "User" role.
        await _userManager.AddToRoleAsync(newUser, "User");

        // Mark invite used and persist.
        invite.UsedAt = DateTime.UtcNow;
        _dbContext.Invites.Update(invite);
        await _dbContext.SaveChangesAsync();

        // Issue tokens for the newly created user.
        var (accessToken, expiration) = await GenerateAccessTokenAsync(newUser);
        var refreshToken = await GenerateRefreshTokenAsync(newUser);

        return new AuthResponse(accessToken, expiration, refreshToken);
    }

    /// <summary>
    /// Validate a refresh token and issue a new access token (keeps the same refresh token).
    /// </summary>
    public async Task<AuthResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

        if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid refresh token");

        var user = await _userManager.FindByIdAsync(stored.UserId) 
            ?? throw new InvalidOperationException("User not found");

        // Issue a new access token but keep the same refresh token.
        var (accessToken, expiration) = await GenerateAccessTokenAsync(user);

        return new AuthResponse(AccessToken: accessToken, expiration, refreshToken);
    }

    /// <summary>
    /// Build an access token (Json Web Token) for the provided user including role claims.
    /// Uses symmetric key from configuration: "Jwt:Key", and issuer/audience from config.
    /// </summary>
    private async Task<(string Token, DateTime Expiration)> GenerateAccessTokenAsync(DiabitsUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            // Use subject claim to carry the user id so controllers can extract ClaimTypes.NameIdentifier or JwtRegisteredClaimNames.Sub.
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Add role claims so authorization attributes like [Authorize(Roles = "Admin")] work.
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Read symmetric key from configuration and create signing credentials.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo);
    }

    /// <summary>
    /// Create and persist a long-lived refresh token tied to the user.
    /// </summary>
    private async Task<string> GenerateRefreshTokenAsync(DiabitsUser user)
    {
        // Use a secure random byte sequence encoded as base64 for the token.
        var unhashedToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            TokenHash = HashToken(unhashedToken),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // TODO Returning object with plain token for client, but DB has hash
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return unhashedToken;
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }
}