using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class StudentVerificationRequest
{
    [Key]
    public int RequestId { get; set; }

    [Required]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [Required]
    [MaxLength(255)]
    public string FrontIdImageUrl { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string BackIdImageUrl { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string CurrentSemester { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByAdminId { get; set; }
    
    [ForeignKey("ReviewedByAdminId")]
    public virtual User? Admin { get; set; }
}
