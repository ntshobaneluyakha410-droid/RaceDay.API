using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Dtos;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly LocalFileService _fileService;

    public AuthController(RaceDayDbContext db, IPasswordHasher hasher, LocalFileService fileService)
    {
        _db = db;
        _hasher = hasher;
        _fileService = fileService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto.Role != "Organiser" && dto.Role != "Participant") return BadRequest("Invalid role");
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email)) return Conflict("Email already registered.");

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

        if (dto.Role == "Organiser")
            _db.Organisers.Add(new Organiser { UserID = user.UserID, Organisation = dto.Organisation ?? "N/A" });
        else
            _db.Participants.Add(new Participant { UserID = user.UserID, DateOfBirth = dto.DateOfBirth ?? DateTime.UtcNow, PhoneNumber = dto.PhoneNumber ?? "N/A" });

        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Register), new { id = user.UserID }, new { user.UserID, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !_hasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("Email", user.Email);

        return Ok(new { user.UserID, user.Email, user.Role });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return NoContent();
    }

    [HttpPost("upload-profile")]
    public async Task<IActionResult> UploadProfile(IFormFile file)
    {
        var userId = HttpContext.Session.GetInt32("UserID");
        var role = HttpContext.Session.GetString("Role");
        if (userId == null) return Unauthorized();
        if (role != "Participant") return Forbid();

        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
        if (participant == null) return NotFound();

        var imageUrl = await _fileService.UploadFileAsync(file);
        participant.ProfileImageUrl = imageUrl;
        await _db.SaveChangesAsync();

        return Ok(new { ImageUrl = imageUrl });
    }
}