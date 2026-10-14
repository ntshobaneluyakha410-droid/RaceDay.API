using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>Returns the current session's UserID or null.</summary>
    protected int? CurrentUserID =>
        HttpContext.Session.GetInt32("UserID");

    /// <summary>Returns the current session's role or null.</summary>
    protected string? CurrentRole =>
        HttpContext.Session.GetString("Role");

    protected bool IsLoggedIn => CurrentUserID is not null;
    protected bool IsOrganiser => CurrentRole == "Organiser";
    protected bool IsParticipant => CurrentRole == "Participant";
}