// DTOs/ChatDtos.cs
public class ConversationDto
{
    public int ConversationId { get; set; }
    public int PartnerId { get; set; }
    public string PartnerName { get; set; }
    public string PartnerAvatar { get; set; }
    public bool IsOnline { get; set; } // Trạng thái online của đối phương
    public string LastMessage { get; set; }
    public DateTime LastMessageAt { get; set; }
    public bool HasUnread { get; set; }
    public bool IsBlockedByMe { get; set; }
    public bool IsBlockedByThem { get; set; }
}

public class MessageDto
{
    public int MessageId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; }
    public string MessageType { get; set; } // "Text", "Image"
    public string MediaUrl { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class SendMessageRequest
{
    public int ReceiverId { get; set; }
    public string Content { get; set; }
    public string MessageType { get; set; } = "Text";
    public string? MediaUrl { get; set; }
}