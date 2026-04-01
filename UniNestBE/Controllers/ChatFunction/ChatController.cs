using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UniNestBE.Services;

namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc phải đăng nhập
    public class ChatController : ControllerBase
    {
        private readonly UniNestDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IPremiumCheckService _premiumCheckService;

        public ChatController(UniNestDbContext context, IHubContext<ChatHub> hubContext, IPremiumCheckService premiumCheckService)
        {
            _context = context;
            _hubContext = hubContext;
            _premiumCheckService = premiumCheckService;
        }

        // Lấy danh sách các cuộc hội thoại
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            // Kiểm tra Premium - Trả về IActionResult trực tiếp nếu không đạt điều kiện
            //var premiumCheck = _premiumCheckService.CheckPremiumAndRedirect(User, this);
            //if (premiumCheck != null) return premiumCheck;

            // 1. Lấy claim UserID
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return Unauthorized("Token không hợp lệ hoặc thiếu UserID");
            }

            var userId = int.Parse(claim.Value);

            // 2. Query danh sách hội thoại
            var conversations = await _context.Conversations.AsNoTracking()
                .Include(c => c.ParticipantOne)
                .Include(c => c.ParticipantTwo)
                .Where(c => c.ParticipantOneID == userId || c.ParticipantTwoID == userId)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            // 3. Lấy danh sách block để check trạng thái
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
        public async Task<IActionResult> GetChatHistory(int partnerId)
        {
            // Kiểm tra Premium
            //var premiumCheck = _premiumCheckService.CheckPremiumAndRedirect(User, this);
            //if (premiumCheck != null) return premiumCheck;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return Unauthorized("Token không hợp lệ hoặc thiếu UserID");
            }

            var userId = int.Parse(claim.Value);

            // Tìm cuộc hội thoại giữa 2 người
            var conversation = await _context.Conversations.AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    (c.ParticipantOneID == userId && c.ParticipantTwoID == partnerId) ||
                    (c.ParticipantOneID == partnerId && c.ParticipantTwoID == userId));

            if (conversation == null) return Ok(new List<MessageDto>());

            // Lấy danh sách tin nhắn
            var messages = await _context.Messages.AsNoTracking()
                .Where(m => m.ConversationID == conversation.ConversationID)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageID,
                    SenderId = m.SenderID,
                    Content = m.Content ?? string.Empty,
                    MessageType = m.MessageType,
                    MediaUrl = m.MediaURL ?? string.Empty,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return Ok(messages);
        }

        // ============ CREATE CONVERSATION ============

        [HttpPost("create-conversation/{partnerId}")]
        public async Task<IActionResult> CreateConversation(int partnerId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return Unauthorized("Token không hợp lệ hoặc thiếu UserID");
            }

            var userId = int.Parse(claim.Value);

            // Tìm conversation hiện có hoặc tạo mới
            var conversation = await _context.Conversations
                .Include(c => c.ParticipantOne)
                .Include(c => c.ParticipantTwo)
                .FirstOrDefaultAsync(c =>
                    (c.ParticipantOneID == userId && c.ParticipantTwoID == partnerId) ||
                    (c.ParticipantOneID == partnerId && c.ParticipantTwoID == userId));

            if (conversation == null)
            {
                // Tạo conversation mới
                conversation = new Conversation
                {
                    ParticipantOneID = userId,
                    ParticipantTwoID = partnerId,
                    LastMessage = "",
                    LastMessageAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
                
                // Reload để get related entities
                await _context.Entry(conversation).Reference(c => c.ParticipantOne).LoadAsync();
                await _context.Entry(conversation).Reference(c => c.ParticipantTwo).LoadAsync();
            }

            // Lấy block status
            var blocks = await _context.UserBlocks.AsNoTracking()
                .Where(b => (b.BlockerID == userId && b.BlockedID == partnerId) || 
                           (b.BlockerID == partnerId && b.BlockedID == userId))
                .ToListAsync();

            var partner = conversation.ParticipantOneID == userId ? conversation.ParticipantTwo : conversation.ParticipantOne;

            bool blockedByMe = blocks.Any(b => b.BlockerID == userId && b.BlockedID == partner.UserId);
            bool blockedByThem = blocks.Any(b => b.BlockerID == partner.UserId && b.BlockedID == userId);

            var dto = new ConversationDto
            {
                ConversationId = conversation.ConversationID,
                PartnerId = partner.UserId,
                PartnerName = partner.FullName ?? "",
                PartnerAvatar = partner.StudentAvatar ?? "",
                IsOnline = partner.IsOnline,
                LastMessage = conversation.LastMessage ?? "",
                LastMessageAt = conversation.LastMessageAt,
                IsBlockedByMe = blockedByMe,
                IsBlockedByThem = blockedByThem
            };

            return Ok(dto);
        }

        // ============ BLOCK / UNBLOCK / REPORT ============

        [HttpPost("block/{partnerId}")]
        public async Task<IActionResult> BlockUser(int partnerId)
        {
            //var premiumCheck = _premiumCheckService.CheckPremiumAndRedirect(User, this);
            //if (premiumCheck != null) return premiumCheck;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Unauthorized();
            var userId = int.Parse(claim.Value);

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

            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateBlockStatus", partnerId, true, true);
            await _hubContext.Clients.User(partnerId.ToString()).SendAsync("UpdateBlockStatus", userId, true, false);

            return Ok(new { message = "User blocked successfully" });
        }

        [HttpPost("unblock/{partnerId}")]
        public async Task<IActionResult> UnblockUser(int partnerId)
        {
            //var premiumCheck = _premiumCheckService.CheckPremiumAndRedirect(User, this);
            //if (premiumCheck != null) return premiumCheck;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Unauthorized();
            var userId = int.Parse(claim.Value);

            var block = await _context.UserBlocks.FirstOrDefaultAsync(b =>
                b.BlockerID == userId && b.BlockedID == partnerId);

            if (block == null)
                return NotFound(new { message = "Block record not found" });

            _context.UserBlocks.Remove(block);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateBlockStatus", partnerId, false, true);
            await _hubContext.Clients.User(partnerId.ToString()).SendAsync("UpdateBlockStatus", userId, false, false);

            return Ok(new { message = "User unblocked successfully" });
        }

        [HttpPost("report/{partnerId}")]
        public async Task<IActionResult> ReportUser(int partnerId)
        {
            //var premiumCheck = _premiumCheckService.CheckPremiumAndRedirect(User, this);
            //if (premiumCheck != null) return premiumCheck;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Unauthorized();
            var userId = int.Parse(claim.Value);

            // Logic report có thể mở rộng thêm ở đây
            Console.WriteLine($"[REPORT] User {userId} reported User {partnerId} at {DateTime.Now}");

            return Ok(new { message = "Report submitted successfully" });
        }
    }
}