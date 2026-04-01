public class ListingEditDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public double AreaSquareMeters { get; set; }
    public bool IsAvailable { get; set; }
    public string? GenderPreference { get; set; }

    //  THÊM
    public string? Address { get; set; }
    public string? District { get; set; }
}