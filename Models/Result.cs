namespace RaceDay.Api.Models;

public class Result
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public Enrolment Enrolment { get; set; } = null!;
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public string ResultStatus { get; set; } = null!;
}