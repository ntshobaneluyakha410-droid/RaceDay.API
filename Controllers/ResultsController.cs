using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api/[controller]")]
public class ResultsController : BaseController
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db) => _db = db;

    /// <summary>Participants view their own results.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyResults()
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsParticipant) return Forbid();

        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.UserID == CurrentUserID);
        if (participant is null) return Forbid();

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(en => en.Participant).ThenInclude(p => p.User)
            .Include(r => r.Enrolment).ThenInclude(en => en.Category).ThenInclude(c => c.Event)
            .Where(r => r.Enrolment.ParticipantID == participant.ParticipantID)
            .Select(r => new ResultResponseDto
            {
                ResultID = r.ResultID,
                EnrolmentID = r.EnrolmentID,
                ParticipantID = r.Enrolment.ParticipantID,
                ParticipantName = r.Enrolment.Participant.User.FirstName + " " +
                                  r.Enrolment.Participant.User.LastName,
                EventName = r.Enrolment.Category.Event.EventName,
                CategoryName = r.Enrolment.Category.CategoryName,
                FinishTime = r.FinishTime,
                Position = r.Position,
                ResultStatus = r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Organisers view all results for one of their events.</summary>
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

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(en => en.Participant).ThenInclude(p => p.User)
            .Include(r => r.Enrolment).ThenInclude(en => en.Category).ThenInclude(c => c.Event)
            .Where(r => r.Enrolment.Category.EventID == eventId)
            .Select(r => new ResultResponseDto
            {
                ResultID = r.ResultID,
                EnrolmentID = r.EnrolmentID,
                ParticipantID = r.Enrolment.ParticipantID,
                ParticipantName = r.Enrolment.Participant.User.FirstName + " " +
                                  r.Enrolment.Participant.User.LastName,
                EventName = r.Enrolment.Category.Event.EventName,
                CategoryName = r.Enrolment.Category.CategoryName,
                FinishTime = r.FinishTime,
                Position = r.Position,
                ResultStatus = r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Organisers capture a result for an enrolment.</summary>
    [HttpPost("enrolment/{enrolmentId}")]
    public async Task<IActionResult> Capture(int enrolmentId, [FromBody] CreateResultDto dto)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var enrolment = await _db.Enrolments
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(en => en.EnrolmentID == enrolmentId);
        if (enrolment is null) return NotFound("Enrolment not found.");
        if (enrolment.Category.Event.OrganiserID != organiser.OrganiserID) return Forbid();

        // Prevent duplicate result for same enrolment
        var existing = await _db.Results
            .FirstOrDefaultAsync(r => r.EnrolmentID == enrolmentId);
        if (existing is not null)
            return Conflict("A result already exists for this enrolment.");

        var result = new Result
        {
            EnrolmentID = enrolmentId,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            ResultStatus = dto.ResultStatus
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyResults), null,
            new { result.ResultID, result.EnrolmentID, result.Position });
    }

    /// <summary>Organisers update a result. Only the owning Organiser can.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateResultDto dto)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(en => en.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(r => r.ResultID == id);
        if (result is null) return NotFound();
        if (result.Enrolment.Category.Event.OrganiserID != organiser.OrganiserID) return Forbid();

        result.FinishTime = dto.FinishTime;
        result.Position = dto.Position;
        result.ResultStatus = dto.ResultStatus;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Organisers delete a result. Only the owning Organiser can.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(en => en.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(r => r.ResultID == id);
        if (result is null) return NotFound();
        if (result.Enrolment.Category.Event.OrganiserID != organiser.OrganiserID) return Forbid();

        _db.Results.Remove(result);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}