using Diabits.API.Data;
using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Diabits.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Services;

public class UserService(DiabitsDbContext dbContext, UserManager<DiabitsUser> userManager, IAuthService authService) : IUserService
{
    private readonly DiabitsDbContext _dbContext = dbContext;

    public async Task<DateTime?> GetLastSuccessSyncForUserAsync(string userId)
    {
        var lastSync = await _dbContext.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.LastSyncSuccess)
            .FirstOrDefaultAsync();
        return (lastSync == default) ? null : lastSync;
    }

    public async Task UpdateLastSuccessSyncForUserAsync(string userId, DateTime newSync) =>
        await _dbContext.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(s => s.SetProperty(u => u.LastSyncSuccess, newSync));
}
