using UniNestFE.Services;

namespace UniNestFE.DTOs
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public int PartnerId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string PartnerAvatar { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        
        // Context Info (Listing)
        public int? ListingId { get; set; }
        public string? ListingTitle { get; set; }
        public decimal? ListingPrice { get; set; }
        public string? ListingAddress { get; set; }
        public string? ListingImage { get; set; }
        public bool IsVerifiedHost { get; set; }
        public bool IsBlockedByMe { get; set; }
        public bool IsBlockedByThem { get; set; }
    }

    public class MessageDto
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text"; // Text, Image, File
        public string MediaUrl { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        
        // Content Moderation
        public ScamAlertInfo? SafetyAlert { get; set; }
    }
    
    public class SendMessageRequest
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";
        public string? MediaUrl { get; set; }
    }
}
