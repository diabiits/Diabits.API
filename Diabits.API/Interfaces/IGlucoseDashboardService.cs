using static Diabits.API.Services.GlucoseDashboardService;

namespace Diabits.API.Interfaces;

public interface IGlucoseDashboardService
{
    Task<DailyGlucoseResponse> GetDailyGlucoseAsync(string userId, DateOnly date);
}
