namespace RaceDay.Api.Models;

public class User
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;   // "Organiser" or "Participant"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Organiser? Organiser { get; set; }
    public Participant? Participant { get; set; }
}