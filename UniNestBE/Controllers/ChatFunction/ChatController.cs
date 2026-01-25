using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Bắt buộc phải đăng nhập
public class ChatController : ControllerBase
{
    private readonly UniNestDbContext _context;

    public ChatController(UniNestDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách các cuộc hội thoại
    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations()
    {
        // 1. Lấy claim ra trước (có thể null)
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        // 2. Kiểm tra nếu null thì trả về Unauthorized
        if (claim == null)
        {
            return Unauthorized("Token không hợp lệ hoặc thiếu UserID");
        }

        // 3. Parse an toàn
        var userId = int.Parse(claim.Value);

        var conversations = await _context.Conversations.AsNoTracking()
            .Include(c => c.ParticipantOne)
            .Include(c => c.ParticipantTwo)
            .Where(c => c.ParticipantOneID == userId || c.ParticipantTwoID == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        var dtos = conversations.Select(c =>
        {
            // Kiểm tra null cho partner để tránh lỗi Crash (CS8602)
            // Lưu ý: Đảm bảo bạn đã .Include(c => c.ParticipantOne) khi query
            var partner = c.ParticipantOneID == userId ? c.ParticipantTwo : c.ParticipantOne;

            return new ConversationDto
            {
                ConversationId = c.ConversationID,
                PartnerId = partner.UserId,

                // Nếu partner null thì fallback về chuỗi rỗng hoặc tên mặc định
                PartnerName = partner.FullName ?? "",

                // SỬA LỖI CS8601 Ở ĐÂY:
                // Nếu Avatar null -> lấy chuỗi rỗng
                PartnerAvatar = partner.StudentAvatar ?? "",

                IsOnline = partner.IsOnline,

                // Nếu LastMessage null -> lấy chuỗi rỗng
                LastMessage = c.LastMessage ?? "",

                LastMessageAt = c.LastMessageAt
            };
        }).ToList();

        return Ok(dtos);
    }

    // Lấy lịch sử tin nhắn của 1 người dùng cụ thể
    [HttpGet("history/{partnerId}")]
    public async Task<ActionResult<List<MessageDto>>> GetChatHistory(int partnerId)
    {
        // 1. Lấy claim ra trước (có thể null)
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        // 2. Kiểm tra nếu null thì trả về Unauthorized
        if (claim == null)
        {
            return Unauthorized("Token không hợp lệ hoặc thiếu UserID");
        }

        // 3. Parse an toàn
        var userId = int.Parse(claim.Value);

        var conversation = await _context.Conversations.AsNoTracking()
            .FirstOrDefaultAsync(c =>
                (c.ParticipantOneID == userId && c.ParticipantTwoID == partnerId) ||
                (c.ParticipantOneID == partnerId && c.ParticipantTwoID == userId));

        if (conversation == null) return Ok(new List<MessageDto>());

        var messages = await _context.Messages.AsNoTracking()
            .Where(m => m.ConversationID == conversation.ConversationID)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                MessageId = m.MessageID,
                SenderId = m.SenderID,

                // SỬA Ở ĐÂY: Nếu null thì trả về chuỗi rỗng ""
                Content = m.Content ?? string.Empty,

                MessageType = m.MessageType,

                // SỬA Ở ĐÂY: Tương tự cho MediaUrl
                MediaUrl = m.MediaURL ?? string.Empty,

                SentAt = m.SentAt,
                IsRead = m.IsRead
            })
            .ToListAsync();

        return Ok(messages);
    }
}