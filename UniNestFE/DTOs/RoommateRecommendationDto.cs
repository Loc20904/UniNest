namespace UniNestFE.DTOs
{
    public class RoommateRecommendationDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public double MatchScore { get; set; } // 0 to 100
        public string Bio { get; set; } = string.Empty;
        
        // Detailed Info
        public string University { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string Gender { get; set; } = "Other";
        
        // Matching Attributes
        public List<string> CommonHabits { get; set; } = new(); // e.g. "Clean", "Quiet"
        public double DistanceToUni { get; set; } // km
        public bool IsSameUniversity { get; set; }
    }
}
