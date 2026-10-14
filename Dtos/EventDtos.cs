namespace RaceDay.Api.Dtos;

public class CreateEventDto
{
    public string EventName { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string EventType { get; set; } = null!;   // "Run", "Walk", "Cycle"
}

public class UpdateEventDto : CreateEventDto { }

public class EventResponseDto
{
    public int EventID { get; set; }
    public string EventName { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = null!;
}