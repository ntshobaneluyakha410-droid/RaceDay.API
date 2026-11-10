using RaceDay.Api.Models;

namespace RaceDay.API.Models;

public class Participant
{
    public int ParticipantID { get; set; }
    public int UserID { get; set; }
    public User User { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? ProfileImageUrl { get; set; } // <-- NEW

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}