using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Dtos;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[Route("api/[controller]")]
public class EnrolmentsController : BaseController
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    [HttpPost("event/{eventId}")]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] CreateEnrolmentDto dto)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsParticipant) return Forbid();

        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserID);
        if (participant is null) return Forbid();

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev is null) return NotFound("Event not found.");

        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryID && c.EventID == eventId);
        if (cat is null) return BadRequest("Category not found for this event.");

        var alreadyEnrolled = await _db.Enrolments.AnyAsync(en => en.ParticipantID == participant.ParticipantID && en.CategoryID == dto.CategoryID);
        if (alreadyEnrolled) return Conflict("You are already enrolled in this category.");

        var enrolment = new Enrolment
        {
            ParticipantID = participant.ParticipantID,
            CategoryID = dto.CategoryID,
            EnrolmentDate = DateTime.UtcNow,
            EnrolmentStatus = "Confirmed"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return Ok(new { enrolment.EnrolmentID });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsParticipant) return Forbid();

        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserID);
        if (participant is null) return Forbid();

        var list = await _db.Enrolments
            .Include(en => en.Participant).ThenInclude(p => p.User)
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .Where(en => en.ParticipantID == participant.ParticipantID)
            .Select(en => new { en.EnrolmentID, en.Category.CategoryName, en.Category.Event.EventName, en.EnrolmentStatus })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev is null) return NotFound("Event not found.");
        if (ev.OrganiserID != organiser.OrganiserID) return Forbid();

        var list = await _db.Enrolments
            .Include(en => en.Participant).ThenInclude(p => p.User)
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .Where(en => en.Category.EventID == eventId)
            .Select(en => new { en.EnrolmentID, en.Participant.User.FirstName, en.Participant.User.LastName, en.Category.CategoryName, en.EnrolmentStatus })
            .ToListAsync();

        return Ok(list);
    }
}