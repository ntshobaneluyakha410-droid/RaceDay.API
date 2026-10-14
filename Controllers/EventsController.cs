using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api/[controller]")]
public class EventsController : BaseController
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db) => _db = db;

    /// <summary>Lists all events. Both roles can view.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");

        var events = await _db.Events
            .Include(e => e.Organiser)
            .ThenInclude(o => o.User)
            .Select(e => new EventResponseDto
            {
                EventID = e.EventID,
                EventName = e.EventName,
                EventDate = e.EventDate,
                Location = e.Location,
                Description = e.Description,
                EventType = e.EventType,
                OrganiserID = e.OrganiserID,
                OrganiserName = e.Organiser.User.FirstName + " " + e.Organiser.User.LastName
            })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>Gets a single event by ID. Both roles can view.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");

        var e = await _db.Events
            .Include(x => x.Organiser).ThenInclude(o => o.User)
            .FirstOrDefaultAsync(x => x.EventID == id);

        if (e is null) return NotFound();

        return Ok(new EventResponseDto
        {
            EventID = e.EventID,
            EventName = e.EventName,
            EventDate = e.EventDate,
            Location = e.Location,
            Description = e.Description,
            EventType = e.EventType,
            OrganiserID = e.OrganiserID,
            OrganiserName = e.Organiser.User.FirstName + " " + e.Organiser.User.LastName
        });
    }

    /// <summary>Creates a new event. Organiser only.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var newEvent = new Event
        {
            EventName = dto.EventName,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Description = dto.Description,
            EventType = dto.EventType,
            OrganiserID = organiser.OrganiserID
        };

        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = newEvent.EventID },
            new { newEvent.EventID, newEvent.EventName });
    }

    /// <summary>Updates an event. Only the owning Organiser can.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev is null) return NotFound();
        if (ev.OrganiserID != organiser.OrganiserID) return Forbid();

        ev.EventName = dto.EventName;
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location;
        ev.Description = dto.Description;
        ev.EventType = dto.EventType;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Deletes an event. Only the owning Organiser can.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev is null) return NotFound();
        if (ev.OrganiserID != organiser.OrganiserID) return Forbid();

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}