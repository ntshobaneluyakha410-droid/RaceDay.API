namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public Participant Participant { get; set; } = null!;
    public int CategoryID { get; set; }
    public Category Category { get; set; } = null!;
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string EnrolmentStatus { get; set; } = "Pending";

    public Result? Result { get; set; }
}