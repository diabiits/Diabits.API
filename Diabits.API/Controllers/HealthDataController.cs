using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Diabits.API.Controllers;

/// <summary>
/// Controller handling health data sync and manual input operations for authenticated users. 
/// </summary>
[Authorize]
[Route("[controller]")]
[ApiController]
public class HealthDataController(IHealthDataService healthDataService) : ControllerBase
{
    private readonly IHealthDataService _healthDataService = healthDataService;

    [HttpGet("manual")]
    public async Task<IActionResult> GetManualInputForDay([FromQuery] DateTime date)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        if (date == default)
            return BadRequest("Invalid request");

        try
        {
            var response = await _healthDataService.GetManualInputForDayAsync(userId, date);
            return Ok(response);
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    /// <summary>
    /// Retrieves all health data (numerics, workouts, and manual inputs) for the authenticated user within a specified time period.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHealthDataForPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        if (startDate == default || endDate == default)
            return BadRequest("Invalid date range");

        if (startDate > endDate)
            return BadRequest("Start date must be before or equal to end date");

        try
        {
            var response = await _healthDataService.GetHealthDataForPeriodAsync(userId, startDate, endDate);
            return Ok(response);
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    /// <summary>
    /// Endpoint to accept manual input (medication and menstruation entries) in batch.
    /// </summary>
    [HttpPost("manual/batch")]
    public async Task<IActionResult> PostManualInput([FromBody] ManualInputRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        if (!(request.Items?.Any() ?? false))
            return BadRequest("No data to save");

        try
        {
            await _healthDataService.AddDataPointsAsync(request, userId);
            return Ok("Manual data added");
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    // Updates multiple manual input entries in a batch. Validates payload and delegates to service.
    [HttpPut("manual/batch")]
    public async Task<IActionResult> BatchUpdateManualInput([FromBody] ManualInputRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        if (!(request.Items?.Any() ?? false))
            return BadRequest("No data to update");

        try
        {
            var updatedCount = await _healthDataService.BatchUpdateManualInputAsync(userId, request.Items);
            return Ok(new { message = "Manual inputs updated", updatedCount });
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    // Deletes multiple manual input entries in a batch. Validates payload and delegates to service.
    [HttpDelete("manual/batch")]
    public async Task<IActionResult> BatchDeleteManualInput([FromBody] BatchDeleteManualInputRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        // Reject empty batch requests early.
        if (!(request.Ids?.Any() ?? false))
            return BadRequest("No IDs to delete");

        try
        {
            var deletedCount = await _healthDataService.BatchDeleteManualInputAsync(userId, request.Ids);
            return Ok(new { message = "Manual inputs deleted", deletedCount });
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }

    [HttpPost("healthConnect")]
    public async Task<IActionResult> PostHealthConnectData([FromBody] HealthConnectRequest request)
    {
        // Extract user id from JWT claims (NameIdentifier). All requests must be from an authenticated user.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        try
        {
            await _healthDataService.AddDataPointsAsync(request, userId);
            return Ok("Sync successful");
        }
        catch (Exception e)
        {
            return Problem(statusCode: 500, detail: "An error occurred");
        }
    }
}