using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Conversation
{
    [Key]
    public int ConversationID { get; set; }

    // Người tham gia 1
    public int ParticipantOneID { get; set; }
    [ForeignKey("ParticipantOneID")]
    public virtual User ParticipantOne { get; set; }

    // Người tham gia 2
    public int ParticipantTwoID { get; set; }
    [ForeignKey("ParticipantTwoID")]
    public virtual User ParticipantTwo { get; set; }

    // Thông tin hiển thị nhanh (Preview)
    public string? LastMessage { get; set; }
    public DateTime LastMessageAt { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Quan hệ 1-n
    public virtual ICollection<Message> Messages { get; set; }
}