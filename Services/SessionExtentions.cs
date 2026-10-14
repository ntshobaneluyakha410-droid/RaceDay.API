namespace RaceDay.Api.Services;

public static class SessionExtensions
{
    public static int? GetUserID(this ISession session) =>
        session.GetInt32("UserID");

    public static string? GetRole(this ISession session) =>
        session.GetString("Role");

    public static bool IsLoggedIn(this ISession session) =>
        session.GetInt32("UserID") is not null;

    public static bool IsOrganiser(this ISession session) =>
        session.GetString("Role") == "Organiser";

    public static bool IsParticipant(this ISession session) =>
        session.GetString("Role") == "Participant";
}