namespace Diabits.API.Models;

/// <summary>
/// Domain entity representing an admin-created invite used for invite-based registration.
/// </summary>
public class Invite
{
    public int Id { get; set; }

    // Recipient email for which the invite was created.
    public string Email { get; set; } = null!;

    // Used by registration flows to validate and tie registrations to a created invite.
    public string Code { get; set; } = Guid.NewGuid().ToString();

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    // When the invite was used. Backed by a private field to enforce single-use.
    public DateTime? UsedAt
    {
        get => _usedAt;
        set
        {
            if (_usedAt != null)
                throw new InvalidOperationException("Invite has already been used.");
            _usedAt = value;
        }
    }
    private DateTime? _usedAt;

    // Navigation to the user who is tied to the invite. Nullable because invites are created before registration of user.
    public DiabitsUser? UsedBy { get; set; }
}