public class RoommateRecommendationDto
{
    public int UserId { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? UniversityName { get; set; }

    // AI Scores
    public double TotalMatchScore { get; set; }
    public List<string> MatchTags { get; set; } = new List<string>();

    // Thông tin phòng (Nếu người này là Host)
    public decimal? MinPrice { get; set; }
    public string? ListingAddress { get; set; }
}

public class ListingRecommendationDto
{
    public int ListingId { get; set; }
    public string? Title { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    public double DistanceToCampusKM { get; set; } // Khoảng cách tới trường
    public double CompatibilityScore { get; set; } // Độ hợp với chủ nhà
}