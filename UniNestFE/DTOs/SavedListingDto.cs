public class SavedListingDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string District { get; set; }
    public string ImageUrl { get; set; }
    public bool IsBookedOut { get; set; }
    public string BadgeText { get; set; }

    // UI only
    public bool IsSelectedForCompare { get; set; }
}