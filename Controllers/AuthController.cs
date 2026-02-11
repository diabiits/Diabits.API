using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diabits.API.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<AuthController> _logger = logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var authResponse = await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, authResponse);
        }
        catch (InvalidOperationException ex)
        {
            // Service throws InvalidOperationException for expected user-facing errors (email already used etc)
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while registering");
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var authResponse = await _authService.LoginAsync(request);
            return Ok(authResponse);
        }
        catch (InvalidOperationException ex)
        {
            // Bad request for known authentication failures surfaced by the service
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while logging in");
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    /// <summary>
    /// Public logout endpoint that accepts a refresh token and asks the service to revoke it.
    /// </summary>
    /// <remarks>
    /// Marked [AllowAnonymous] so clients can post the refresh token even if current access token expired. 
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while logging out");
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    /// <summary>
    /// Public endpoint to exchange a refresh token for new access token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var authResponse = await _authService.RefreshAccessTokenAsync(request.RefreshToken);
            return Ok(authResponse);
        }
        catch (InvalidOperationException e)
        {
            // Invalid/expired refresh token results in 401 to signal the client to reauthenticate.
            _logger.LogWarning(e, "Invalid refresh token attempt");
            return Unauthorized(new { message = e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while refreshing token");
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    /// <summary>
    /// Protected endpoint used to verify the current access token is valid.
    /// </summary>
    /// <remarks>Will throw as soon as the token is invalid/expired, otherwise returns 200 OK.</remarks>
    [Authorize]
    [HttpGet("checkToken")]
    public IActionResult CheckToken() => Ok();
}