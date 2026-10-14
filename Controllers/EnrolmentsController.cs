
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api/[controller]")]
public class EnrolmentsController : BaseController
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    /// <summary>Participants enrol in an event by picking a category.</summary>
    [HttpPost("event/{eventId}")]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] CreateEnrolmentDto dto)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsParticipant) return Forbid();

        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.UserID == CurrentUserID);
        if (participant is null) return Forbid();

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev is null) return NotFound("Event not found.");

        var cat = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryID && c.EventID == eventId);
        if (cat is null) return BadRequest("Category not found for this event.");

        // Prevent duplicate enrolment
        var alreadyEnrolled = await _db.Enrolments.AnyAsync(en =>
            en.ParticipantID == participant.ParticipantID &&
            en.CategoryID == dto.CategoryID);
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

        return CreatedAtAction(nameof(GetMyEnrolments), null,
            new { enrolment.EnrolmentID, enrolment.ParticipantID, enrolment.CategoryID });
    }

    /// <summary>Participants view all their own enrolments.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsParticipant) return Forbid();

        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.UserID == CurrentUserID);
        if (participant is null) return Forbid();

        var list = await _db.Enrolments
            .Include(en => en.Participant).ThenInclude(p => p.User)
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .Where(en => en.ParticipantID == participant.ParticipantID)
            .Select(en => new EnrolmentResponseDto
            {
                EnrolmentID = en.EnrolmentID,
                ParticipantID = en.ParticipantID,
                ParticipantName = en.Participant.User.FirstName + " " + en.Participant.User.LastName,
                CategoryID = en.CategoryID,
                CategoryName = en.Category.CategoryName,
                EventID = en.Category.EventID,
                EventName = en.Category.Event.EventName,
                EnrolmentDate = en.EnrolmentDate,
                EnrolmentStatus = en.EnrolmentStatus
            })
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>Organisers view all enrolments for one of their events.</summary>
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
            .Select(en => new EnrolmentResponseDto
            {
                EnrolmentID = en.EnrolmentID,
                ParticipantID = en.ParticipantID,
                ParticipantName = en.Participant.User.FirstName + " " + en.Participant.User.LastName,
                CategoryID = en.CategoryID,
                CategoryName = en.Category.CategoryName,
                EventID = en.Category.EventID,
                EventName = en.Category.Event.EventName,
                EnrolmentDate = en.EnrolmentDate,
                EnrolmentStatus = en.EnrolmentStatus
            })
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>Deletes an enrolment. Participant can cancel their own.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");

        var enrolment = await _db.Enrolments.FirstOrDefaultAsync(en => en.EnrolmentID == id);
        if (enrolment is null) return NotFound();

        // Participant can delete own; Organiser can delete any in their events
        if (IsParticipant)
        {
            var participant = await _db.Participants
                .FirstOrDefaultAsync(p => p.UserID == CurrentUserID);
            if (participant is null || enrolment.ParticipantID != participant.ParticipantID)
                return Forbid();
        }
        else if (IsOrganiser)
        {
            var organiser = await _db.Organisers
                .FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
            if (organiser is null) return Forbid();

            var eventForCat = await _db.Categories
                .Where(c => c.CategoryID == enrolment.CategoryID)
                .Select(c => c.Event)
                .FirstOrDefaultAsync();
            if (eventForCat is null || eventForCat.OrganiserID != organiser.OrganiserID)
                return Forbid();
        }
        else
        {
            return Forbid();
        }

        _db.Enrolments.Remove(enrolment);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}