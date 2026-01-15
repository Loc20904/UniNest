using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int UserId { get; set; }

    public string Role { get; set; } = "user"; // user, admin

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string? PhoneNumber { get; set; }
    public string? StudentAvatar { get; set; }

    // Navigation Properties (Quan hệ)
    public ICollection<Listing> Listings { get; set; }
}