using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthController(RaceDayDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    /// <summary>Registers a new Organiser or Participant account.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto.Role != "Organiser" && dto.Role != "Participant")
            return BadRequest("Role must be 'Organiser' or 'Participant'.");

        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Email already registered.");

        // Create the base User
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password),
            Role = dto.Role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Create the role-specific record
        if (dto.Role == "Organiser")
        {
            _db.Organisers.Add(new Organiser
            {
                UserID = user.UserID,
                Organisation = dto.Organisation ?? "N/A"
            });
        }
        else
        {
            _db.Participants.Add(new Participant
            {
                UserID = user.UserID,
                DateOfBirth = dto.DateOfBirth ?? DateTime.UtcNow,
                PhoneNumber = dto.PhoneNumber ?? "N/A"
            });
        }
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new { id = user.UserID },
            new { user.UserID, user.Email, user.Role });
    }

    /// <summary>Logs in a registered user and creates a session.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !_hasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        // Create the session
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("Email", user.Email);

        return Ok(new { user.UserID, user.Email, user.Role });
    }

    /// <summary>Logs out the current user (clears the session).</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return NoContent();
    }
}