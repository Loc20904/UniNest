using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Listing
{
    [Key]
    public int ListingId { get; set; }

    public int OwnerId { get; set; }
    [ForeignKey("OwnerId")]
    public User? Owner { get; set; }

    [Required]
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")] // Tiền tệ
    public decimal Price { get; set; }

    public double AreaSquareMeters { get; set; }

    public bool IsAvailable { get; set; } = true;

    [MaxLength(20)]
    public string ApprovalStatus { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

    [MaxLength(20)]
    public string GenderPreference { get; set; } = "Any"; // Male, Female, Any

    public List<Amenity> Amenities { get; set; } = new List<Amenity>();

    public List<LifestyleHabit> LifestyleHabits { get; set; } = new List<LifestyleHabit>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ExpireAt { get; set; } = DateTime.Now.AddDays(30);

    // Quan hệ 1-1 hoặc 1-n: Một bài đăng có 1 địa chỉ
    public int AddressId { get; set; }
    public virtual Address? Address { get; set; }

    public int? PropertyTypeId { get; set; }
    [ForeignKey("PropertyTypeId")]
    public PropertyType? PropertyType { get; set; }

    public ICollection<ListingImage>? Images { get; set; }

}