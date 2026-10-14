namespace RaceDay.Api.Dtos;

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = null!;
    public double DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto { }

public class CategoryResponseDto
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = null!;
    public double DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public int EventID { get; set; }
}