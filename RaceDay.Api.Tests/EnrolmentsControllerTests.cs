using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class EnrolmentsControllerTests
{
    private static EnrolmentsController BuildController(
        RaceDay.Api.Data.RaceDayDbContext db,
        int? userId = null,
        string? role = null)
    {
        var controller = new EnrolmentsController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };

        if (userId is not null)
            httpContext.Session.SetInt32("UserID", userId.Value);
        if (role is not null)
            httpContext.Session.SetString("Role", role);

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    // --- Seed helpers ---

    private static async Task<(User user, Participant participant)> SeedParticipant(
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

    private static async Task<(User user, Organiser organiser, Event ev, Category cat)>
        SeedOrganiserWithEventAndCategory(
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

        var org = new Organiser { UserID = user.UserID, Organisation = "Test" };
        db.Organisers.Add(org);
        await db.SaveChangesAsync();

        var ev = new Event
        {
            EventName = "Test Event",
            EventDate = DateTime.UtcNow.AddDays(30),
            Location = "JHB",
            Description = "Desc",
            EventType = "Run",
            OrganiserID = org.OrganiserID
        };
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var cat = new Category
        {
            CategoryName = "Senior",
            DistanceKm = 10,
            EntryFee = 100m,
            EventID = ev.EventID
        };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        return (user, org, ev, cat);
    }

    // --- Tests ---

    [Fact]
    public async Task Enrol_AsParticipant_ReturnsCreated_AndRecordsEnrolment()
    {
        using var db = TestDb.Create();
        var (_, p) = await SeedParticipant(db);
        var (_, _, ev, cat) = await SeedOrganiserWithEventAndCategory(db);

        var controller = BuildController(db, userId: p.UserID, role: "Participant");

        var result = await controller.Enrol(ev.EventID, new CreateEnrolmentDto
        {
            CategoryID = cat.CategoryID
        });

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(db.Enrolments);

        var saved = db.Enrolments.First();
        Assert.Equal(p.ParticipantID, saved.ParticipantID);
        Assert.Equal(cat.CategoryID, saved.CategoryID);
    }

    [Fact]
    public async Task Enrol_AsOrganiser_ReturnsForbid()
    {
        using var db = TestDb.Create();
        var (orgUser, _, ev, cat) = await SeedOrganiserWithEventAndCategory(db);

        var controller = BuildController(db, userId: orgUser.UserID, role: "Organiser");

        var result = await controller.Enrol(ev.EventID, new CreateEnrolmentDto
        {
            CategoryID = cat.CategoryID
        });

        Assert.IsType<ForbidResult>(result);
        Assert.Empty(db.Enrolments);
    }

    [Fact]
    public async Task Enrol_NotLoggedIn_ReturnsUnauthorized()
    {
        using var db = TestDb.Create();
        var (_, _, ev, cat) = await SeedOrganiserWithEventAndCategory(db);

        var controller = BuildController(db); // no session

        var result = await controller.Enrol(ev.EventID, new CreateEnrolmentDto
        {
            CategoryID = cat.CategoryID
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Enrol_Duplicate_ReturnsConflict()
    {
        using var db = TestDb.Create();
        var (_, p) = await SeedParticipant(db);
        var (_, _, ev, cat) = await SeedOrganiserWithEventAndCategory(db);

        var controller = BuildController(db, userId: p.UserID, role: "Participant");

        var dto = new CreateEnrolmentDto { CategoryID = cat.CategoryID };

        await controller.Enrol(ev.EventID, dto);   // first
        var second = await controller.Enrol(ev.EventID, dto); // duplicate

        Assert.IsType<ConflictObjectResult>(second);
        Assert.Single(db.Enrolments);
    }

    [Fact]
    public async Task Enrol_InvalidCategory_ReturnsBadRequest()
    {
        using var db = TestDb.Create();
        var (_, p) = await SeedParticipant(db);
        var (_, _, ev, _) = await SeedOrganiserWithEventAndCategory(db);

        var controller = BuildController(db, userId: p.UserID, role: "Participant");

        var result = await controller.Enrol(ev.EventID, new CreateEnrolmentDto
        {
            CategoryID = 9999 // doesn't exist
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(db.Enrolments);
    }

    [Fact]
    public async Task GetMyEnrolments_ReturnsOnlyOwnEnrolments()
    {
        using var db = TestDb.Create();
        var (_, p) = await SeedParticipant(db);
        var (_, _, ev, cat) = await SeedOrganiserWithEventAndCategory(db);

        // Seed an enrolment for our participant
        db.Enrolments.Add(new Enrolment
        {
            ParticipantID = p.ParticipantID,
            CategoryID = cat.CategoryID,
            EnrolmentDate = DateTime.UtcNow,
            EnrolmentStatus = "Confirmed"
        });
        await db.SaveChangesAsync();

        var controller = BuildController(db, userId: p.UserID, role: "Participant");
        var result = await controller.GetMyEnrolments();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<EnrolmentResponseDto>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetByEvent_AsOwningOrganiser_ReturnsAllEnrolments()
    {
        using var db = TestDb.Create();
        var (_, p) = await SeedParticipant(db);
        var (orgUser, _, ev, cat) = await SeedOrganiserWithEventAndCategory(db);

        db.Enrolments.Add(new Enrolment
        {
            ParticipantID = p.ParticipantID,
            CategoryID = cat.CategoryID,
            EnrolmentDate = DateTime.UtcNow,
            EnrolmentStatus = "Confirmed"
        });
        await db.SaveChangesAsync();

        var controller = BuildController(db, userId: orgUser.UserID, role: "Organiser");
        var result = await controller.GetByEvent(ev.EventID);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<EnrolmentResponseDto>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetByEvent_AsNonOwningOrganiser_ReturnsForbid()
    {
        using var db = TestDb.Create();
        var (_, _, ev, _) = await SeedOrganiserWithEventAndCategory(db);

        // Second organiser, different user
        var otherUser = new User
        {
            FirstName = "Other",
            LastName = "Org",
            Email = "other@test.com",
            PasswordHash = "x",
            Role = "Organiser"
        };
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();
        db.Organisers.Add(new Organiser { UserID = otherUser.UserID, Organisation = "Other" });
        await db.SaveChangesAsync();

        var controller = BuildController(db, userId: otherUser.UserID, role: "Organiser");
        var result = await controller.GetByEvent(ev.EventID);

        Assert.IsType<ForbidResult>(result);
    }
}