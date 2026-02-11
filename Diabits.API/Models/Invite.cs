namespace Diabits.API.Models;

/// <summary>
/// Domain entity representing an admin-created invite used for invite-based registration.
/// </summary>
public class Invite
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Code { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DiabitsUser? UsedBy { get; set; }

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
}