using Diabits.API.DTOs;
using Diabits.API.Models;

namespace Diabits.API.Interfaces;

public interface IInviteService
{
    Task<List<InviteResponse>> GetAllInvitesAsync();
    Task<Invite> CreateInviteAsync(CreateInviteRequest request);
}
