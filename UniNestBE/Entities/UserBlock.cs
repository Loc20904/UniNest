using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserBlock
{
    [Key]
    public int BlockID { get; set; }

    public int BlockerID { get; set; } // Người chặn
    [ForeignKey("BlockerID")]
    public virtual User Blocker { get; set; }

    public int BlockedID { get; set; } // Người bị chặn
    [ForeignKey("BlockedID")]
    public virtual User Blocked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}