using System.ComponentModel.DataAnnotations;

public class University
{
    [Key]
    public int UniId { get; set; }

    [Required]
    [MaxLength(150)]
    public string UniName { get; set; }

    // Navigation property: Một trường ĐH có thể có 1 địa chỉ định vị
    // Trong Address.cs bạn đã có UniversityId
    public virtual ICollection<Address> Addresses { get; set; }
}