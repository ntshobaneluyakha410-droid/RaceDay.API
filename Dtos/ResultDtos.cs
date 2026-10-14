namespace RaceDay.Api.Dtos;

public class CreateResultDto
{
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public string ResultStatus { get; set; } = "Finished";
}

public class ResultResponseDto
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = null!;
    public string EventName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public string ResultStatus { get; set; } = null!;
}