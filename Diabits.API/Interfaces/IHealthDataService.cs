using Diabits.API.DTOs;
using Diabits.API.DTOs.HealthDataPoints;

namespace Diabits.API.Interfaces;

public interface IHealthDataService
{
    Task<ManualInputResponse> GetManualInputForDayAsync(string userId, DateTime date);
    Task AddDataPointsAsync(HealthConnectRequest request, string userId);
    Task AddDataPointsAsync(ManualInputRequest request, string userId);
    Task<int> BatchUpdateManualInputAsync(string userId, IEnumerable<ManualInputDto> inputDtos);
    Task<int> BatchDeleteManualInputAsync(string userId, IEnumerable<int> ids);
}