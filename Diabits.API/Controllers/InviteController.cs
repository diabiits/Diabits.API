using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diabits.API.Controllers;

/// <summary>
/// Admin-only controller that exposes invite management endpoints. 
/// </summary>
[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class InviteController(IInviteService inviteService) : ControllerBase
{
    private readonly IInviteService _inviteService = inviteService;

    [HttpGet]
    public async Task<IActionResult> GetAllInvites()
    {
        try
        {
            var invites = await _inviteService.GetAllInvitesAsync();
            return Ok(invites);
        }
        catch (Exception)
        {
            return Problem(statusCode: 500, detail: "An unexpected error occurred ");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvite([FromBody] CreateInviteRequest request)
    {
        try
        {
            var invite = await _inviteService.CreateInviteAsync(request);
            return Created(string.Empty, invite);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Problem(statusCode: 500, detail: "An unexpected error occurred ");
        }
    }
}