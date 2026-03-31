using System.ComponentModel.DataAnnotations;

public class ReviewSubmitDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}

public class ReviewResponseDto
{
    public int ReviewId { get; set; }
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerAvatar { get; set; } = string.Empty;
    
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserReputationDto
{
    public int TargetUserId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    
    public List<ReviewResponseDto> RecentReviews { get; set; } = new();
}
