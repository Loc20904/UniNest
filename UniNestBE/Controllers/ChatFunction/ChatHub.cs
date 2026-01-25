using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ChatHub : Hub
{
    private readonly UniNestDbContext _context;

    public ChatHub(UniNestDbContext context)
    {
        _context = context;
    }

    // Hàm Client gọi để gửi tin nhắn
    public async Task SendMessage(SendMessageRequest request)
    {
        if (int.TryParse(Context.UserIdentifier, out int senderId))
        {
            var receiverId = request.ReceiverId;

            // 1. KIỂM TRA BLOCK (AI-03 / Safety)
            bool isBlocked = await _context.UserBlocks.AsNoTracking().AnyAsync(b =>
                (b.BlockerID == receiverId && b.BlockedID == senderId) || // Mình bị nó chặn
                (b.BlockerID == senderId && b.BlockedID == receiverId));  // Mình chặn nó

            if (isBlocked)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Không thể gửi tin nhắn cho người dùng này.");
                return;
            }

            // 2. TÌM HOẶC TẠO CONVERSATION
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.ParticipantOneID == senderId && c.ParticipantTwoID == receiverId) ||
                    (c.ParticipantOneID == receiverId && c.ParticipantTwoID == senderId));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    ParticipantOneID = senderId,
                    ParticipantTwoID = receiverId,
                    LastMessage = request.MessageType == "Image" ? "[Hình ảnh]" : request.Content,
                    LastMessageAt = DateTime.Now
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }
            else
            {
                conversation.LastMessage = request.MessageType == "Image" ? "[Hình ảnh]" : request.Content;
                conversation.LastMessageAt = DateTime.Now;
            }

            // 3. LƯU TIN NHẮN
            var msg = new Message
            {
                ConversationID = conversation.ConversationID,
                SenderID = senderId,
                Content = request.Content,
                MessageType = request.MessageType,
                MediaURL = request.MediaUrl,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // 4. GỬI REAL-TIME CHO RECEIVER
            // Lưu ý: UserId trong SignalR phải map đúng với UserId trong DB (cần cấu hình Auth)
            var msgDto = new MessageDto
            {
                MessageId = msg.MessageID,
                SenderId = senderId,
                Content = msg.Content,
                MessageType = msg.MessageType,
                MediaUrl = msg.MediaURL ?? "",
                SentAt = msg.SentAt
            };

            // Gửi cho người nhận
            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", msgDto);

            // Gửi lại cho người gửi (để UI cập nhật ngay lập tức mà ko cần reload)
            await Clients.Caller.SendAsync("ReceiveMessage", msgDto);
        }
        else
        {
            // 1. Ghi log để Developer biết (Backend)
            Console.WriteLine($"[Lỗi ChatHub] UserIdentifier không hợp lệ hoặc Null. Giá trị nhận được: '{Context.UserIdentifier}'");

            // 2. Báo lỗi cho Client (Frontend) biết để hiện thông báo
            await Clients.Caller.SendAsync("ReceiveError", "Lỗi xác thực: Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.");

            // 3. (Tùy chọn) Ngắt kết nối ngay lập tức vì user không hợp lệ
            Context.Abort();
        }
    }

    public override async Task OnConnectedAsync()
    {
        // Update User Online Status
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user != null)
            {
                user.IsOnline = true;
                await _context.SaveChangesAsync();
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Update User Offline
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user != null)
            {
                user.IsOnline = false;
                user.LastActiveAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}