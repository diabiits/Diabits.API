using Diabits.API.DTOs;

namespace Diabits.API.Interfaces;

public interface IUserService
{
    Task<DateTime?> GetLastSuccessSyncForUserAsync(string userId);
    Task UpdateLastSuccessSyncForUserAsync(string userId, DateTime newSync);
}