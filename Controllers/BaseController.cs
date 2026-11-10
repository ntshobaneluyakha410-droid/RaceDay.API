using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected int? CurrentUserID => HttpContext.Session.GetInt32("UserID");
    protected string? CurrentRole => HttpContext.Session.GetString("Role");

    protected bool IsLoggedIn => CurrentUserID is not null;
    protected bool IsOrganiser => CurrentRole == "Organiser";
    protected bool IsParticipant => CurrentRole == "Participant"; // THIS FIXES THE ERROR
}