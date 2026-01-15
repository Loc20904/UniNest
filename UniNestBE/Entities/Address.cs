using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Address
{
    [Key]
    public int AddressId { get; set; }

    // ---------------------------------------------------------
    // XỬ LÝ QUAN HỆ POLYMORPHIC (TARGET ID) THEO CÁCH EF CORE
    // ---------------------------------------------------------
    // Thay vì chỉ dùng TargetID int chung chung, ta tách ra để dễ Join bảng

    public int? ListingId { get; set; } // Khóa ngoại trỏ về Listing
    [ForeignKey("ListingId")]
    public Listing? Listing { get; set; }

    // Nếu sau này map cho trường ĐH
    public int? UniversityId { get; set; }

    // ---------------------------------------------------------

    [Required]
    public string FullAddress { get; set; } // Địa chỉ hiển thị tìm kiếm

    public string? City { get; set; } = "Đà Nẵng";
    public string? District { get; set; } // Quận

    // TỌA ĐỘ (Quan trọng cho Map Marker)
    // Type name để đảm bảo độ chính xác cao cho GPS
    [Column(TypeName = "decimal(10, 8)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(11, 8)")]
    public decimal Longitude { get; set; }
}