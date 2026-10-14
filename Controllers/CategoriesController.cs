
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api/[controller]")]
public class CategoriesController : BaseController
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db) => _db = db;

    /// <summary>Lists all categories for a given event. Both roles can view.</summary>
    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");

        var exists = await _db.Events.AnyAsync(e => e.EventID == eventId);
        if (!exists) return NotFound("Event not found.");

        var cats = await _db.Categories
            .Where(c => c.EventID == eventId)
            .Select(c => new CategoryResponseDto
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                DistanceKm = c.DistanceKm,
                EntryFee = c.EntryFee,
                EventID = c.EventID
            })
            .ToListAsync();

        return Ok(cats);
    }

    /// <summary>Gets a single category by ID. Both roles can view.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");

        var c = await _db.Categories.FirstOrDefaultAsync(x => x.CategoryID == id);
        if (c is null) return NotFound();

        return Ok(new CategoryResponseDto
        {
            CategoryID = c.CategoryID,
            CategoryName = c.CategoryName,
            DistanceKm = c.DistanceKm,
            EntryFee = c.EntryFee,
            EventID = c.EventID
        });
    }

    /// <summary>Creates a category for an event. Only the owning Organiser can.</summary>
    [HttpPost("event/{eventId}")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev is null) return NotFound("Event not found.");
        if (ev.OrganiserID != organiser.OrganiserID) return Forbid();

        var cat = new Category
        {
            CategoryName = dto.CategoryName,
            DistanceKm = dto.DistanceKm,
            EntryFee = dto.EntryFee,
            EventID = eventId
        };

        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = cat.CategoryID },
            new { cat.CategoryID, cat.CategoryName });
    }

    /// <summary>Updates a category. Only the owning Organiser can.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var cat = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (cat is null) return NotFound();
        if (cat.Event.OrganiserID != organiser.OrganiserID) return Forbid();

        cat.CategoryName = dto.CategoryName;
        cat.DistanceKm = dto.DistanceKm;
        cat.EntryFee = dto.EntryFee;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Deletes a category. Only the owning Organiser can.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn) return Unauthorized("Please log in.");
        if (!IsOrganiser) return Forbid();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserID);
        if (organiser is null) return Forbid();

        var cat = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (cat is null) return NotFound();
        if (cat.Event.OrganiserID != organiser.OrganiserID) return Forbid();

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}