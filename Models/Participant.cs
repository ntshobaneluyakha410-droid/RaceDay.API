namespace RaceDay.Api.Models;

public class Participant
{
    public int ParticipantID { get; set; }
    public int UserID { get; set; }
    public User User { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = null!;

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}