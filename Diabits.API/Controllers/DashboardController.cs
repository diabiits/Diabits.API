using System.Security.Claims;

using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Diabits.API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diabits.API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    //TODO Remove bucketMinutes
    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline([FromQuery] DateTime date, [FromQuery] int bucketMinutes = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        if (date == default) return BadRequest("date is required");
        if (bucketMinutes <= 0 || bucketMinutes > 60) return BadRequest("bucketMinutes must be 1-60");

        try
        {
            var response = await _dashboardService.GetTimelineAsync(userId, date, bucketMinutes);
            return Ok(response);
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

}
