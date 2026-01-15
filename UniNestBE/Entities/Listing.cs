using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Listing
{
    [Key]
    public int ListingId { get; set; }

    public int OwnerId { get; set; }
    [ForeignKey("OwnerId")]
    public User Owner { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")] // Tiền tệ
    public decimal Price { get; set; }

    public double AreaSquareMeters { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Quan hệ 1-1 hoặc 1-n: Một bài đăng có 1 địa chỉ
    public Address? Address { get; set; }

    public ICollection<ListingImage> Images { get; set; }
}