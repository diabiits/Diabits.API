namespace Diabits.API.Models;

public class RefreshToken
{    
    public int Id { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public string UserId { get; set; } = null!;
    public DiabitsUser User { get; set; } = null!;
}
