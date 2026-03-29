using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.SignalR;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Bắt buộc phải đăng nhập
public class ChatController : ControllerBase
{
    private readonly UniNestDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(UniNestDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
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

        var blocks = await _context.UserBlocks.AsNoTracking()
            .Where(b => b.BlockerID == userId || b.BlockedID == userId)
            .ToListAsync();

        var dtos = conversations.Select(c =>
        {
            var partner = c.ParticipantOneID == userId ? c.ParticipantTwo : c.ParticipantOne;

            bool blockedByMe = blocks.Any(b => b.BlockerID == userId && b.BlockedID == partner.UserId);
            bool blockedByThem = blocks.Any(b => b.BlockerID == partner.UserId && b.BlockedID == userId);

            return new ConversationDto
            {
                ConversationId = c.ConversationID,
                PartnerId = partner.UserId,
                PartnerName = partner.FullName ?? "",
                PartnerAvatar = partner.StudentAvatar ?? "",
                IsOnline = partner.IsOnline,
                LastMessage = c.LastMessage ?? "",
                LastMessageAt = c.LastMessageAt,
                IsBlockedByMe = blockedByMe,
                IsBlockedByThem = blockedByThem
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

    // ============ BLOCK / UNBLOCK / REPORT ============

    [HttpPost("block/{partnerId}")]
    public async Task<IActionResult> BlockUser(int partnerId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();
        var userId = int.Parse(claim.Value);

        // Kiểm tra đã block chưa
        var existing = await _context.UserBlocks.FirstOrDefaultAsync(b =>
            b.BlockerID == userId && b.BlockedID == partnerId);

        if (existing != null)
            return Ok(new { message = "User already blocked" });

        var block = new UserBlock
        {
            BlockerID = userId,
            BlockedID = partnerId,
            CreatedAt = DateTime.Now
        };
        _context.UserBlocks.Add(block);
        await _context.SaveChangesAsync();

        // Gửi thông báo SignalR cho cả 2 người
        await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateBlockStatus", partnerId, true, true); // (partnerId, isBlocked, isBlockedByMe)
        await _hubContext.Clients.User(partnerId.ToString()).SendAsync("UpdateBlockStatus", userId, true, false); // Về phía partner, họ bị block (isBlockedByMe = false)

        return Ok(new { message = "User blocked successfully" });
    }

    [HttpPost("unblock/{partnerId}")]
    public async Task<IActionResult> UnblockUser(int partnerId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();
        var userId = int.Parse(claim.Value);

        var block = await _context.UserBlocks.FirstOrDefaultAsync(b =>
            b.BlockerID == userId && b.BlockedID == partnerId);

        if (block == null)
            return NotFound(new { message = "Block record not found" });

        _context.UserBlocks.Remove(block);
        await _context.SaveChangesAsync();
        
        // Gửi thông báo SignalR cho cả 2 người
        await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateBlockStatus", partnerId, false, true); 
        await _hubContext.Clients.User(partnerId.ToString()).SendAsync("UpdateBlockStatus", userId, false, false); 

        return Ok(new { message = "User unblocked successfully" });
    }

    [HttpPost("report/{partnerId}")]
    public async Task<IActionResult> ReportUser(int partnerId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();
        var userId = int.Parse(claim.Value);

        // TODO: Thêm bảng UserReports để lưu báo cáo chi tiết
        // Tạm thời log lại
        Console.WriteLine($"[REPORT] User {userId} reported User {partnerId} at {DateTime.Now}");

        return Ok(new { message = "Report submitted successfully" });
    }
}