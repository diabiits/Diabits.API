using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Diabits.API.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("lastSync")]
    public async Task<IActionResult> GetLastSyncTime()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            DateTime? lastSync = await _userService.GetLastSuccessSyncForUserAsync(userId);

            return lastSync == null ? NotFound() : Ok(new LastSyncResponse(LastSyncAt: lastSync));
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    [HttpPut("lastSync")]
    public async Task<IActionResult> UpdateLastSyncTime([FromBody] LastSyncRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        if (request.SyncTime == default) return BadRequest("Invalid request");

        try
        {
            await _userService.UpdateLastSuccessSyncForUserAsync(userId, request.SyncTime);
            return Ok("Last sync time updated");
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    [HttpPut("updateAccount")]
    [Authorize]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var accessToken = await _userService.UpdateAccount(userId, request);
            //TODO Refactor
            return Ok(new AuthResponse(accessToken, "TESTING"));
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }
}