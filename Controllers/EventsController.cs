using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.API.Data;
using RaceDay.API.Dtos;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[Route("api/[controller]")]
public class EventsController : BaseController
{
    private readonly RaceDayDbContext _db;
    private readonly LocalFileService _fileService;

    public EventsController(RaceDayDbContext db, LocalFileService fileService)
    {
        _db = db;
        _fileService = fileService;
    }

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
            }).ToListAsync();

        return Ok(events);
    }

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
        return CreatedAtAction(nameof(GetAll), new { id = newEvent.EventID }, newEvent);
    }

    [HttpPost("{id}/upload-banner")]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        if (!IsLoggedIn) return Unauthorized();
        if (!IsOrganiser) return Forbid();

        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        var imageUrl = await _fileService.UploadFileAsync(file);
        ev.BannerImageUrl = imageUrl;
        await _db.SaveChangesAsync();

        return Ok(new { ImageUrl = imageUrl });
    }
}