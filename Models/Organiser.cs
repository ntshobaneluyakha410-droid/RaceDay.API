namespace RaceDay.Api.Models;

public class Organiser
{
    public int OrganiserID { get; set; }
    public int UserID { get; set; }
    public User User { get; set; } = null!;
    public string Organisation { get; set; } = null!;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
