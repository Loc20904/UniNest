using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Review
{
    [Key]
    public int ReviewId { get; set; }

    public int ReviewerId { get; set; } // Người đánh giá
    [ForeignKey("ReviewerId")]
    public virtual User Reviewer { get; set; }

    public int TargetUserId { get; set; } // Người được đánh giá
    [ForeignKey("TargetUserId")]
    public virtual User TargetUser { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}