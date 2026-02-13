using Diabits.API.Data;
using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Diabits.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Services;

public class UserService(DiabitsDbContext dbContext, UserManager<DiabitsUser> userManager) : IUserService
{
    private readonly DiabitsDbContext _dbContext = dbContext;
    private readonly UserManager<DiabitsUser> _userManager = userManager;

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

    public async Task UpdateAccount(string userId, UpdateAccountRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        var wantsUsernameChange = !string.IsNullOrWhiteSpace(request.NewUsername);
        var wantsPasswordChange = !string.IsNullOrWhiteSpace(request.NewPassword);

        if (!wantsUsernameChange && !wantsPasswordChange)
            return;

        var currentPasswordOk = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!currentPasswordOk)
            throw new UnauthorizedAccessException("Current password is incorrect");

        if (wantsUsernameChange)
        {
            var existing = await _userManager.FindByNameAsync(request.NewUsername!);
            if (existing is not null && existing.Id != user.Id)
                throw new InvalidOperationException("Username is already taken.");

            var usernameResult = await _userManager.SetUserNameAsync(user, request.NewUsername!);
            ThrowIfFailed(usernameResult);
        }

        if (wantsPasswordChange)
        {
            var passwordResult = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword!);

            ThrowIfFailed(passwordResult);
        }
    }
    private static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded) return;

        var msg = string.Join(" ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException(msg);
    }
}
