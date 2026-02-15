using Diabits.API.Data;
using Diabits.API.DTOs;
using Diabits.API.Interfaces;
using Diabits.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Services;

public class InviteService(DiabitsDbContext dbContext) : IInviteService
{
    private readonly DiabitsDbContext _dbContext = dbContext;

    public async Task<List<InviteResponse>> GetAllInvitesAsync()
    {
        return await _dbContext.Invites
            .AsNoTracking()
            .Select(i => new InviteResponse(
                i.Email, 
                i.Code, 
                i.CreatedAt, 
                i.UsedAt, 
                i.UsedBy != null ? i.UsedBy.UserName : null
            )).ToListAsync();
    }

    public async Task<Invite> CreateInviteAsync(CreateInviteRequest request)
    {
        var existingInvite = await _dbContext.Invites.FirstOrDefaultAsync(i => i.Email == request.Email);
        if (existingInvite != null)
            throw new InvalidOperationException("Invite already exists for specified email");

        var invite = new Invite
        {
            Email = request.Email
        };

        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync();

        return invite;
    }
}