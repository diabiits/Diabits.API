using Diabits.API.DTOs;
using Diabits.API.Services;

namespace Diabits.API.Interfaces;

public interface ITimelineDashboardService
{
    Task<TimelineChartResponse> GetTimelineAsync(string userId, DateTime date);
}
