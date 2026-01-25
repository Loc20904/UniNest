using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Message
{
    [Key]
    public int MessageID { get; set; }

    public int ConversationID { get; set; }
    [ForeignKey("ConversationID")]
    public virtual Conversation Conversation { get; set; }

    public int SenderID { get; set; }
    [ForeignKey("SenderID")]
    public virtual User Sender { get; set; }

    public string? Content { get; set; }

    // FE-06: Media Sharing support
    [MaxLength(20)]
    public string MessageType { get; set; } = "Text"; // Text, Image, Video

    [MaxLength(500)]
    public string? MediaURL { get; set; }

    // FE-06: Read receipts & Status
    public bool IsRead { get; set; } = false;
    public bool IsFlagged { get; set; } = false; // AI Check
    public bool IsDeleted { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.Now;
}