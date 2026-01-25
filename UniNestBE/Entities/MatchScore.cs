using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MatchScore
{
    [Key]
    public int MatchId { get; set; }

    public int UserAId { get; set; }
    [ForeignKey("UserAId")]
    public virtual User UserA { get; set; }

    public int UserBId { get; set; }
    [ForeignKey("UserBId")]
    public virtual User UserB { get; set; }

    public float CompatibilityScore { get; set; } // 0.0 đến 100.0

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}