using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniNestBE.Entities
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        public int ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }

        public int? TargetUserId { get; set; }
        [ForeignKey("TargetUserId")]
        public User? TargetUser { get; set; }

        public int? TargetListingId { get; set; }
        [ForeignKey("TargetListingId")]
        public Listing? TargetListing { get; set; }

        [Required]
        [MaxLength(100)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; } = "Standard"; // "High", "Medium", "Standard"

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Resolved", "Dismissed"
        
        [MaxLength(50)]
        public string ActionTaken { get; set; } = "None"; // "Warned", "Banned", "Dismissed", "None"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
