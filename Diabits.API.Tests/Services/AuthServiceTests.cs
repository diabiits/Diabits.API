using Diabits.API.Configuration;
using Diabits.API.Data;
using Diabits.API.DTOs;
using Diabits.API.Models;
using Diabits.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Diabits.API.Tests.Services;

public sealed class AuthServiceTests : IDisposable
{
    private const string JwtKey = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly12345678";
    private const string JwtIssuer = "TestIssuer";
    private const string JwtAudience = "TestAudience";

    private readonly Mock<UserManager<DiabitsUser>> _userManager = CreateUserManagerMock();
    private readonly DiabitsDbContext _db;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _db = CreateInMemoryDb();
        _authService = new AuthService(_userManager.Object, _db, CreateJwtConfiguration().Object);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens_AndPersistsRefreshToken()
    {
        var user = CreateUser(id: "id");
        var request = new LoginRequest("user", "Password1!");

        SetupValidLogin(user, request, roles: ["User", "Admin"]);

        var result = await _authService.LoginAsync(request);

        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);

        var stored = await _db.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        Assert.True(stored.ExpiresAt > DateTime.UtcNow.AddDays(29));
    }

    [Fact]
    public async Task LoginAsync_AccessToken_ContainsSubjectAndRoles()
    {
        var user = CreateUser(id: "id");
        var request = new LoginRequest("user", "Password1!");

        SetupValidLogin(user, request, roles: ["User", "Admin"]);

        var result = await _authService.LoginAsync(request);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        Assert.Equal(user.Id, token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Theory]
    [InlineData(null, "Password1!", "Invalid credentials")]
    [InlineData("user", "WrongPassword!", "Invalid credentials")]
    public async Task LoginAsync_WithInvalidCredentials_ThrowsInvalidOperationException(string? username, string password, string expectedMessage)
    {
        var user = username is null ? null : CreateUser(id: "id", username: username);
        var request = new LoginRequest(username ?? "invalid", password);

        _userManager.Setup(um => um.FindByNameAsync(request.Username)).ReturnsAsync(user);

        if (user is not null)
            _userManager.Setup(um => um.CheckPasswordAsync(user, request.Password)).ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.LoginAsync(request));
        Assert.Equal(expectedMessage, ex.Message);

        if (user is null)
            _userManager.Verify(um => um.CheckPasswordAsync(It.IsAny<DiabitsUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithValidInvite_CreatesUser_AddsRole_AndMarksInviteUsed()
    {
        var invite = await SeedInvite(code: "INVITE123", email: "user@example.com", usedAt: null);
        var request = new RegisterRequest("user", "Password1!", "user@example.com", invite.Code);

        DiabitsUser? createdUser = null;

        _userManager.Setup(um => um.FindByNameAsync(request.Username)).ReturnsAsync((DiabitsUser)null!);
        _userManager
            .Setup(um => um.CreateAsync(It.IsAny<DiabitsUser>(), request.Password))
            .Callback<DiabitsUser, string>((u, _) => createdUser = u)
            .ReturnsAsync(IdentityResult.Success);

        _userManager.Setup(um => um.AddToRoleAsync(It.IsAny<DiabitsUser>(), "User")).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(um => um.GetRolesAsync(It.IsAny<DiabitsUser>())).ReturnsAsync(["User"]);

        var result = await _authService.RegisterAsync(request);

        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);

        Assert.NotNull(createdUser);
        Assert.Equal(invite.Id, createdUser.InviteId);

        _userManager.Verify(um => um.CreateAsync(It.IsAny<DiabitsUser>(), request.Password), Times.Once);
        _userManager.Verify(um => um.AddToRoleAsync(It.IsAny<DiabitsUser>(), "User"), Times.Once);

        var updatedInvite = await _db.Invites.FindAsync(invite.Id);
        Assert.NotNull(updatedInvite!.UsedAt);
    }

    [Theory]
    [InlineData("INVALID", "user@example.com", false, null, "Invalid invite")]
    [InlineData("USED123", "user@example.com", true, -1, "Invalid invite")]
    [InlineData("INVITE123", "wrong@example.com", false, null, "Invalid invite")]
    public async Task RegisterAsync_WithInvalidInviteScenarios_ThrowsInvalidOperationException(
        string inviteCode, string email, bool inviteExists, int? usedDaysAgo, string expectedMessage)
    {
        if (inviteExists)
            await SeedInvite(inviteCode, "user@example.com", usedDaysAgo.HasValue ? DateTime.UtcNow.AddDays(usedDaysAgo.Value) : null);

        var request = new RegisterRequest("user", email, "Password1!", inviteCode);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ThrowsInvalidOperationException()
    {
        await SeedInvite("INVITE123", "user@example.com", usedAt: null);

        var existingUser = CreateUser(username: "existinguser");
        var request = new RegisterRequest("existinguser", "Password1!", "user@example.com", "INVITE123");

        _userManager.Setup(um => um.FindByNameAsync(request.Username)).ReturnsAsync(existingUser);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        Assert.Equal("Username already taken", ex.Message);

        _userManager.Verify(um => um.CreateAsync(It.IsAny<DiabitsUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithValidRefreshToken_RemovesTokenFromDatabase()
    {
        var refreshToken = "valid_token_12345";
        var tokenHash = refreshToken.HashToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = "user123",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });
        await _db.SaveChangesAsync();

        await _authService.LogoutAsync(refreshToken);

        Assert.False(await _db.RefreshTokens.AnyAsync(rt => rt.TokenHash == tokenHash));
    }

    [Theory]
    [InlineData("invalid_token")]
    [InlineData("nonexistent_token")]
    public async Task LogoutAsync_WithInvalidOrNonExistentToken_DoesNotThrow(string refreshToken)
    {
        await _authService.LogoutAsync(refreshToken);
        await _authService.LogoutAsync(refreshToken);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_WithValidToken_ReturnsNewAccessToken_AndSameRefreshToken()
    {
        var user = CreateUser(id: "user");
        var refreshToken = "valid_refresh_token";
        await SeedRefreshToken(user.Id, refreshToken, expiresAt: DateTime.UtcNow.AddDays(30));

        _userManager.Setup(um => um.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(["User"]);

        var result = await _authService.RefreshAccessTokenAsync(refreshToken);

        Assert.NotNull(result.AccessToken);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(refreshToken, result.RefreshToken);
    }

    [Theory]
    [InlineData("invalid_token", false, false, "Invalid refresh token")]
    [InlineData("expired_token", true, true, "Invalid refresh token")]
    [InlineData("orphaned_token", true, false, "User not found")]
    public async Task RefreshAccessTokenAsync_WithInvalidScenarios_ThrowsInvalidOperationException(
        string refreshToken, bool tokenExists, bool isExpired, string expectedMessage)
    {
        if (tokenExists)
        {
            await SeedRefreshToken(
                userId: "id",
                refreshToken: refreshToken,
                expiresAt: isExpired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(30),
                createdAt: DateTime.UtcNow.AddDays(-31));

            if (!isExpired)
                _userManager.Setup(um => um.FindByIdAsync("id")).ReturnsAsync((DiabitsUser)null!);
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RefreshAccessTokenAsync(refreshToken));
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task GeneratedTokens_HaveExpectedExpirations_AndRefreshTokenStoredAsHash()
    {
        var user = CreateUser(id: "id");
        var request = new LoginRequest("user", "Password1!");

        SetupValidLogin(user, request, roles: ["User"]);

        var result = await _authService.LoginAsync(request);

        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.True(result.ExpiresAt <= DateTime.UtcNow.AddHours(3).AddSeconds(10));

        var stored = await _db.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        Assert.NotEqual(result.RefreshToken, stored.TokenHash);
        Assert.Equal(44, stored.TokenHash.Length);
        Assert.True(stored.ExpiresAt > DateTime.UtcNow.AddDays(29));
    }

    // Helpers

    private static Mock<UserManager<DiabitsUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<DiabitsUser>>();
        return new Mock<UserManager<DiabitsUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    private static DiabitsDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<DiabitsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DiabitsDbContext(options);
    }

    private static Mock<IConfiguration> CreateJwtConfiguration()
    {
        var cfg = new Mock<IConfiguration>();
        cfg.Setup(c => c["Jwt:Key"]).Returns(JwtKey);
        cfg.Setup(c => c["Jwt:Issuer"]).Returns(JwtIssuer);
        cfg.Setup(c => c["Jwt:Audience"]).Returns(JwtAudience);
        return cfg;
    }

    private static DiabitsUser CreateUser(string id = "id", string username = "user", string email = "user@example.com")
        => new() { Id = id, UserName = username, Email = email };

    private void SetupValidLogin(DiabitsUser user, LoginRequest request, string[] roles)
    {
        _userManager.Setup(um => um.FindByNameAsync(request.Username)).ReturnsAsync(user);
        _userManager.Setup(um => um.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _userManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);
    }

    private async Task<Invite> SeedInvite(string code, string email, DateTime? usedAt)
    {
        var invite = new Invite { Code = code, Email = email, UsedAt = usedAt };
        _db.Invites.Add(invite);
        await _db.SaveChangesAsync();
        return invite;
    }

    private async Task SeedRefreshToken(string userId, string refreshToken, DateTime expiresAt, DateTime? createdAt = null)
    {
        _db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = refreshToken.HashToken(),
            UserId = userId,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            ExpiresAt = expiresAt
        });
        await _db.SaveChangesAsync();
    }
}
