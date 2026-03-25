using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Amenity
{
    [Key]
    public int AmenityId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public string Icon { get; set; }

    // Bảng trung gian ảo (EF Core tự động sinh bảng ListingAmenity)
    public List<Listing> Listings { get; set; } = new List<Listing>();
}
