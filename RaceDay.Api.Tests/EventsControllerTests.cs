using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using Xunit;

namespace RaceDay.Api.Tests;

public class EventsControllerTests
{
    // Helper: builds a controller with a session representing a logged-in user
    private static EventsController BuildController(
        RaceDay.Api.Data.RaceDayDbContext db,
        int? userId = null,
        string? role = null)
    {
        var controller = new EventsController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };

        if (userId is not null)
            httpContext.Session.SetInt32("UserID", userId.Value);
        if (role is not null)
            httpContext.Session.SetString("Role", role);

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    // ---- SEED HELPERS ----

    private static async Task<(User organiserUser, Organiser organiser)> SeedOrganiser(
        RaceDay.Api.Data.RaceDayDbContext db, string email = "org@test.com")
    {
        var user = new User
        {
            FirstName = "Org",
            LastName = "User",
            Email = email,
            PasswordHash = "x",
            Role = "Organiser"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var org = new Organiser { UserID = user.UserID, Organisation = "Test Org" };
        db.Organisers.Add(org);
        await db.SaveChangesAsync();
        return (user, org);
    }

    private static async Task<(User participantUser, Participant participant)> SeedParticipant(
        RaceDay.Api.Data.RaceDayDbContext db, string email = "part@test.com")
    {
        var user = new User
        {
            FirstName = "Part",
            LastName = "User",
            Email = email,
            PasswordHash = "x",
            Role = "Participant"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var p = new Participant
        {
            UserID = user.UserID,
            DateOfBirth = new DateTime(1995, 1, 1),
            PhoneNumber = "082"
        };
        db.Participants.Add(p);
        await db.SaveChangesAsync();
        return (user, p);
    }

    // ---- TESTS ----

    [Fact]
    public async Task GetAll_NotLoggedIn_ReturnsUnauthorized()
    {
        using var db = TestDb.Create();
        var controller = BuildController(db); // no session

        var result = await controller.GetAll();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Create_AsParticipant_ReturnsForbid()
    {
        using var db = TestDb.Create();
        var (pUser, _) = await SeedParticipant(db);

        var controller = BuildController(db, userId: pUser.UserID, role: "Participant");

        var result = await controller.Create(new CreateEventDto
        {
            EventName = "Test",
            EventDate = DateTime.UtcNow.AddDays(30),
            Location = "JHB",
            Description = "Desc",
            EventType = "Run"
        });

        Assert.IsType<ForbidResult>(result);
        Assert.Empty(db.Events);
    }

    [Fact]
    public async Task Create_AsOrganiser_ReturnsCreated()
    {
        using var db = TestDb.Create();
        var (oUser, _) = await SeedOrganiser(db);

        var controller = BuildController(db, userId: oUser.UserID, role: "Organiser");

        var result = await controller.Create(new CreateEventDto
        {
            EventName = "Comrades",
            EventDate = DateTime.UtcNow.AddDays(60),
            Location = "Pietermaritzburg",
            Description = "90km",
            EventType = "Run"
        });

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(db.Events);
        Assert.Equal("Comrades", db.Events.First().EventName);
    }

    [Fact]
    public async Task Update_ByOtherOrganiser_ReturnsForbid()
    {
        using var db = TestDb.Create();

        // Organiser A creates event
        var (userA, orgA) = await SeedOrganiser(db, "a@test.com");
        db.Events.Add(new Event
        {
            EventName = "A's Event",
            EventDate = DateTime.UtcNow,
            Location = "X",
            Description = "X",
            EventType = "Run",
            OrganiserID = orgA.OrganiserID
        });
        await db.SaveChangesAsync();

        // Organiser B tries to update it
        var (userB, _) = await SeedOrganiser(db, "b@test.com");
        var controller = BuildController(db, userId: userB.UserID, role: "Organiser");

        var result = await controller.Update(1, new UpdateEventDto
        {
            EventName = "Hacked",
            EventDate = DateTime.UtcNow,
            Location = "Y",
            Description = "Y",
            EventType = "Walk"
        });

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Delete_ByOwningOrganiser_ReturnsNoContent()
    {
        using var db = TestDb.Create();
        var (user, org) = await SeedOrganiser(db);

        db.Events.Add(new Event
        {
            EventName = "Delete Me",
            EventDate = DateTime.UtcNow,
            Location = "X",
            Description = "X",
            EventType = "Run",
            OrganiserID = org.OrganiserID
        });
        await db.SaveChangesAsync();

        var controller = BuildController(db, userId: user.UserID, role: "Organiser");

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.Events);
    }
}