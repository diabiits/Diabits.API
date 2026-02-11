using Microsoft.AspNetCore.Identity;

namespace Diabits.API.Models;

/// <summary>
/// Application user model that extends ASP.NET Core Identity's IdentityUser.
/// </summary>
public class DiabitsUser : IdentityUser
{
    // Timestamp of the user's last successful HealthConnect sync.
    // Updated by HealthDataService when a client's sync completes successfully.
    public DateTime LastSyncSuccess { get; set; }

    // Optional FK to the Invite that the user used to register (null for seeded/admin users).
    public int? InviteId { get; set; }

    // Navigation property to the Invite entity. Nullable because the admin isn't invited, but seeded.
    public Invite? Invite { get; set; }
}
