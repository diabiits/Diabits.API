using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Diabits.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Diabits.API.Controllers;

//TODO Cancellation tokens
[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

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
            return BadRequest(error: ex.Message);
        }
        catch (Exception e)
        {
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
            return BadRequest(error: ex.Message);
        }
        catch (Exception e)
        {
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
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    [HttpPut("UpdateCredentials")]
    [Authorize]
    public async Task<IActionResult> UpdateCredentials([FromBody] UpdateCredentialsRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var accessToken = await _authService.UpdateCredentialsAsync(userId, request);
            //TODO Refactor
            return Ok(new AuthResponse(accessToken, "TESTING"));
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    //TODO Maybe the refresh token can stay serverside? Does the user need it?
    /// <summary>
    /// Public endpoint to exchange a refresh token for new access token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var authResponse = await _authService.RefreshAccessTokenAsync(userId, request.RefreshToken);
            return Ok(authResponse);
        }
        catch (InvalidOperationException e)
        {
            // Invalid/expired refresh token results in 401 to signal the client to reauthenticate.
            return Unauthorized(value: e.Message);
        }
        catch (Exception e)
        {
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