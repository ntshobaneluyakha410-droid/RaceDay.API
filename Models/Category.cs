namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public Event Event { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public double DistanceKm { get; set; }
    public decimal EntryFee { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}