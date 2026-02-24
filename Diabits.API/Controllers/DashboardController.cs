using System.Security.Claims;

using Diabits.API.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diabits.API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ITimelineDashboardService _timelineDashboardService;
    private readonly IGlucoseDashboardService _glucoseDashboardService;

    public DashboardController(ITimelineDashboardService timelineDashboardService, IGlucoseDashboardService glucoseDashboardService)
    {
        _timelineDashboardService = timelineDashboardService;
        _glucoseDashboardService = glucoseDashboardService;
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline([FromQuery] DateTime date)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        if (date == default) return BadRequest("date is required");

        try
        {
            var response = await _timelineDashboardService.GetTimelineAsync(userId, date);
            return Ok(response);
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    [HttpGet("glucose/daily")]
    public async Task<IActionResult> GetDailyGlucose([FromQuery] DateOnly date)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        if (date == default) return BadRequest("date is required");

        try
        {
            var response = await _glucoseDashboardService.GetDailyGlucoseAsync(userId, date);
            return Ok(response);
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

}
