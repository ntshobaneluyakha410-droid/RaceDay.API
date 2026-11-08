using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthControllerTests
{
    // ---------------- REGISTER ----------------

    [Fact]
    public async Task Register_Participant_ReturnsCreated()
    {
        using var db = TestDb.Create();
        var controller = new AuthController(db, new PasswordHasher());

        var result = await controller.Register(new RegisterDto
        {
            FirstName = "Thabo",
            LastName = "Ndlovu",
            Email = "thabo@test.com",
            Password = "Pass123!",
            Role = "Participant",
            DateOfBirth = new DateTime(1995, 6, 15),
            PhoneNumber = "0821234567"
        });

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(db.Users);
        Assert.Single(db.Participants);
    }

    [Fact]
    public async Task Register_Organiser_ReturnsCreated()
    {
        using var db = TestDb.Create();
        var controller = new AuthController(db, new PasswordHasher());

        var result = await controller.Register(new RegisterDto
        {
            FirstName = "Sarah",
            LastName = "Mokoena",
            Email = "sarah@test.com",
            Password = "Pass123!",
            Role = "Organiser",
            Organisation = "Gauteng Athletics"
        });

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(db.Users);
        Assert.Single(db.Organisers);
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        using var db = TestDb.Create();
        var controller = new AuthController(db, new PasswordHasher());

        var result = await controller.Register(new RegisterDto
        {
            FirstName = "X",
            LastName = "Y",
            Email = "x@test.com",
            Password = "Pass123!",
            Role = "Admin" // not allowed
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        using var db = TestDb.Create();
        var controller = new AuthController(db, new PasswordHasher());

        var dto = new RegisterDto
        {
            FirstName = "Thabo",
            LastName = "Ndlovu",
            Email = "dup@test.com",
            Password = "Pass123!",
            Role = "Participant",
            DateOfBirth = DateTime.UtcNow,
            PhoneNumber = "082"
        };

        await controller.Register(dto);
        var second = await controller.Register(dto);

        Assert.IsType<ConflictObjectResult>(second);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task Register_PasswordIsHashed_NotStoredPlain()
    {
        using var db = TestDb.Create();
        var controller = new AuthController(db, new PasswordHasher());

        await controller.Register(new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "hash@test.com",
            Password = "MySecret123!",
            Role = "Participant",
            DateOfBirth = DateTime.UtcNow,
            PhoneNumber = "082"
        });

        var user = db.Users.First();
        Assert.NotEqual("MySecret123!", user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash); // BCrypt hash prefix
    }

    // ---------------- LOGIN ----------------

    [Fact]
    public async Task Login_CorrectCredentials_ReturnsOk_AndCreatesSession()
    {
        using var db = TestDb.Create();
        var hasher = new PasswordHasher();

        // Seed a user directly
        db.Users.Add(new Models.User
        {
            FirstName = "Thabo",
            LastName = "Ndlovu",
            Email = "thabo@test.com",
            PasswordHash = hasher.Hash("Pass123!"),
            Role = "Participant"
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db, hasher);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await controller.Login(new LoginDto
        {
            Email = "thabo@test.com",
            Password = "Pass123!"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, httpContext.Session.GetInt32("UserID"));
        Assert.Equal("Participant", httpContext.Session.GetString("Role"));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        using var db = TestDb.Create();
        var hasher = new PasswordHasher();

        db.Users.Add(new Models.User
        {
            FirstName = "Thabo",
            LastName = "Ndlovu",
            Email = "thabo@test.com",
            PasswordHash = hasher.Hash("CorrectPass!"),
            Role = "Participant"
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db, hasher);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { Session = new TestSession() }
        };

        var result = await controller.Login(new LoginDto
        {
            Email = "thabo@test.com",
            Password = "WrongPass!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        using var db = TestDb.Create();
        var controller = new AuthController(db, new PasswordHasher());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { Session = new TestSession() }
        };

        var result = await controller.Login(new LoginDto
        {
            Email = "nobody@test.com",
            Password = "Whatever"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}