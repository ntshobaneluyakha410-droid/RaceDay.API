namespace RaceDay.Api.Models;

public class Event
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public Organiser Organiser { get; set; } = null!;
    public string EventName { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string EventType { get; set; } = null!;   // ← CHANGED from "Event" to "EventType"
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}