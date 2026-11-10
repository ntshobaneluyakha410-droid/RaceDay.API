namespace RaceDay.API.Dtos;

public class RegisterDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? Organisation { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}