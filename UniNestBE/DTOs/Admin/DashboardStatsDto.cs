using System;
using System.Collections.Generic;

namespace UniNestBE.DTOs.Admin
{
    public class DashboardStatsDto
    {
        public int TotalAccounts { get; set; }
        public int PendingStudentVerifications { get; set; }
        public int PendingListings { get; set; }
        public decimal TotalRevenue { get; set; }
        
        public List<DashboardVerificationRequestDto> RecentVerifications { get; set; } = new();
        public List<DashboardListingDto> RecentListings { get; set; } = new();
    }

    public class DashboardVerificationRequestDto
    {
        public int RequestId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UniversityName { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string FrontIdImageUrl { get; set; } = string.Empty;
        public string BackIdImageUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    public class DashboardListingDto
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
