using Diabits.API.Controllers;
using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Diabits.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    // Register endpoint tests
    [Fact]
    public async Task Register_WithValidRequest_ReturnsCreatedWithAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest("user", "user@example.com", "Password1!", "code");
        var expectedResponse = new AuthResponse("access_token", "refresh_token");

        _mockAuthService
            .Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
        var authResponse = Assert.IsType<AuthResponse>(objectResult.Value);
        Assert.Equal(expectedResponse.AccessToken, authResponse.AccessToken);
        Assert.Equal(expectedResponse.RefreshToken, authResponse.RefreshToken);
    }

    [Fact]
    public async Task Register_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest("user", "user@example.com", "Password1!", "invalidCode");
        var expectedError = "Invalid invite";

        _mockAuthService
            .Setup(s => s.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException(expectedError));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var message = Assert.IsType<string>(badRequest.Value);
        Assert.Equal(expectedError, message);
    }

    [Fact]
    public async Task Register_WhenServiceThrowsException_ReturnsProblem()
    {
        // Arrange
        var request = new RegisterRequest("user", "user@example.com", "Password1!", "code");

        _mockAuthService
            .Setup(s => s.RegisterAsync(request))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Login endpoint tests
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new LoginRequest("user", "Password1!");
        var expectedResponse = new AuthResponse("access_token", "refresh_token");

        _mockAuthService
            .Setup(s => s.LoginAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var authResponse = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(expectedResponse.AccessToken, authResponse.AccessToken);
        Assert.Equal(expectedResponse.RefreshToken, authResponse.RefreshToken);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("invaliduser", "Password1!");
        var expectedError = "Invalid credentials";

        _mockAuthService
            .Setup(s => s.LoginAsync(request))
            .ThrowsAsync(new InvalidOperationException(expectedError));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var message = Assert.IsType<string>(badRequest.Value);
        Assert.Equal(expectedError, message);
    }

    [Fact]
    public async Task Login_WhenServiceThrowsException_ReturnsProblem()
    {
        // Arrange
        var request = new LoginRequest("user", "Password1!");

        _mockAuthService
            .Setup(s => s.LoginAsync(request))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Logout endpoint tests
    [Fact]
    public async Task Logout_WithValidRefreshToken_ReturnsOk()
    {
        // Arrange
        var request = new RefreshTokenRequest("valid_refresh_token");

        _mockAuthService
            .Setup(s => s.LogoutAsync(request.RefreshToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout(request);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Logout_WhenServiceThrowsException_ReturnsProblem()
    {
        // Arrange
        var request = new RefreshTokenRequest("some_token");

        _mockAuthService
            .Setup(s => s.LogoutAsync(request.RefreshToken))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Logout(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    //// RefreshToken endpoint tests
    //[Fact]
    //public async Task RefreshToken_WithValidRefreshToken_ReturnsOkWithAuthResponse()
    //{
    //    // Arrange
    //    var request = new RefreshTokenRequest("valid_refresh_token");
    //    var expectedResponse = new AuthResponse("new_access_token", "valid_refresh_token");

    //    _mockAuthService
    //        .Setup(s => s.RefreshAccessTokenAsync("id", request.RefreshToken))
    //        .ReturnsAsync(expectedResponse);

    //    // Act
    //    var result = await _controller.RefreshToken(request);

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    var authResponse = Assert.IsType<AuthResponse>(okResult.Value);
    //    Assert.Equal(expectedResponse.AccessToken, authResponse.AccessToken);
    //    Assert.Equal(expectedResponse.RefreshToken, authResponse.RefreshToken);
    //}

    //[Fact]
    //public async Task RefreshToken_WithInvalidRefreshToken_ReturnsUnauthorized()
    //{
    //    // Arrange
    //    var request = new RefreshTokenRequest("invalid_token");
    //    var expectedError = "Invalid refresh token attempt";

    //    _mockAuthService
    //        .Setup(s => s.RefreshAccessTokenAsync("id", request.RefreshToken))
    //        .ThrowsAsync(new InvalidOperationException(expectedError));

    //    // Act
    //    var result = await _controller.RefreshToken(request);

    //    // Assert
    //    var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
    //    var message = Assert.IsType<string>(unauthorizedResult.Value); 
    //    Assert.Equal(expectedError, message);
    //}

    //[Fact]
    //public async Task RefreshToken_WhenServiceThrowsException_ReturnsProblem()
    //{
    //    // Arrange
    //    var request = new RefreshTokenRequest("some_token");

    //    _mockAuthService
    //        .Setup(s => s.RefreshAccessTokenAsync("id", request.RefreshToken))
    //        .ThrowsAsync(new Exception("Database error"));

    //    // Act
    //    var result = await _controller.RefreshToken(request);

    //    // Assert
    //    var objectResult = Assert.IsType<ObjectResult>(result);
    //    Assert.Equal(500, objectResult.StatusCode);
    //}

    // CheckToken endpoint tests
    [Fact]
    public void CheckToken_WithValidAuthorization_ReturnsOk()
    {
        // Act
        var result = _controller.CheckToken();

        // Assert
        Assert.IsType<OkResult>(result);
    }
}