namespace RaceDay.Api.Dtos;

public class CreateEnrolmentDto
{
    public int CategoryID { get; set; }
}

public class EnrolmentResponseDto
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = null!;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = null!;
    public int EventID { get; set; }
    public string EventName { get; set; } = null!;
    public DateTime EnrolmentDate { get; set; }
    public string EnrolmentStatus { get; set; } = null!;
}