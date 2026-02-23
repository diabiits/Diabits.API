using Diabits.API.DTOs;
using Diabits.API.Services;

namespace Diabits.API.Interfaces;

public interface IDashboardService
{
    Task<TimelineResponse> GetTimelineAsync(string userId, DateTime date, int bucketMinutes);

}
