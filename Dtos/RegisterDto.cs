namespace RaceDay.Api.Dtos;

public class RegisterDto
{
    // --- Common fields (both roles) ---
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;   // "Organiser" or "Participant"

    // --- Organiser-only field ---
    public string? Organisation { get; set; }

    // --- Participant-only fields ---
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
}