using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Address
{
    [Key]
    public int AddressId { get; set; }

    public int? ListingId { get; set; }
    [ForeignKey("ListingId")]
    public Listing? Listing { get; set; }

    public int? UniversityId { get; set; }


    [ForeignKey("UniversityId")]
    public virtual University? University { get; set; }

    [Required]
    public string? FullAddress { get; set; }
    public string? City { get; set; } = "Đà Nẵng";
    public string? District { get; set; }

    [Column(TypeName = "decimal(10, 8)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(11, 8)")]
    public decimal Longitude { get; set; }
}