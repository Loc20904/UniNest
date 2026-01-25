using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Request
{
    [Key]
    public int RequestId { get; set; }

    public int SenderId { get; set; } // Người xin ở ghép
    [ForeignKey("SenderId")]
    public virtual User Sender { get; set; }

    public int ReceiverId { get; set; } // Chủ phòng
    [ForeignKey("ReceiverId")]
    public virtual User Receiver { get; set; }

    public int ListingId { get; set; }
    [ForeignKey("ListingId")]
    public virtual Listing Listing { get; set; }

    public string? Message { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "pending"; // pending, accepted, rejected, canceled

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}