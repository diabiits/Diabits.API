using System.Net.Http.Json;

using Diabits.API.Configuration;
using Diabits.API.Data;
using Diabits.API.DTOs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class AuthIntegrationTests : IClassFixture<DiabitsWebApplicationFactory>
{
    private readonly DiabitsWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public AuthIntegrationTests(DiabitsWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokensAndSavesRefreshTokenToDb()
    {
        // Arrange
        const string username = "user";
        const string password = "Password1!";

        // Act
        var response = await _httpClient.PostAsJsonAsync("/Auth/login", new LoginRequest(username, password));

        // Assert that we received an access token and a refresh token
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.NotNull(auth.AccessToken);
        Assert.NotNull(auth.RefreshToken);

        // Assert that the refresh token is saved in the database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DiabitsDbContext>();
        
        var user = await db.Users.FirstAsync(u => u.UserName == username);
        var savedToken = await db.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);

        Assert.NotNull(savedToken);
        Assert.Equal(auth.RefreshToken.HashToken(), savedToken!.TokenHash);
    }
}
