using Diabits.API.DTOs;
using Diabits.API.DTOs.HealthDataPoints;

namespace Diabits.API.Interfaces;

public interface IHealthDataService
{
    Task<ManualInputResponse> GetManualInputForDayAsync(string userId, DateTime date);
    Task<HealthDataResponse> GetHealthDataForPeriodAsync(string userId, DateTime startDate, DateTime endDate);
    Task AddDataPointsAsync(HealthConnectRequest request, string userId);
    Task AddDataPointsAsync(ManualInputRequest request, string userId);
    Task<int> BatchUpdateManualInputAsync(string userId, IEnumerable<ManualInputDto> inputDtos);
    Task<int> BatchDeleteManualInputAsync(string userId, IEnumerable<int> ids);
}