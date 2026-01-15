using System.ComponentModel.DataAnnotations;

public class ListingImage
{
    [Key]
    public int ImageId { get; set; }
    public int ListingId { get; set; }
    public string ImageUrl { get; set; }
    public bool IsPrimary { get; set; }
}