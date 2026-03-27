using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniNestBE.Services;

namespace UniNestBE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UniNestDbContext _context;
        private readonly IEmailService _emailService;

        public AdminController(UniNestDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public class VerifyRequestDto
        {
            public int RequestId { get; set; }
            public bool IsApproved { get; set; }
            public string? RejectReason { get; set; }
        }

        [HttpPost("verify-student")]
        public async Task<IActionResult> VerifyStudent([FromBody] VerifyRequestDto dto)
        {
            try
            {
                var request = await _context.StudentVerificationRequests
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.RequestId == dto.RequestId);

                if (request == null)
                {
                    return NotFound(new { message = "Không tìm thấy yêu cầu xác minh." });
                }

                if (request.Status != "Pending")
                {
                    return BadRequest(new { message = "Yêu cầu này đã được xử lý." });
                }

                request.Status = dto.IsApproved ? "Verified" : "Rejected";
                request.ReviewedAt = DateTime.UtcNow;

                // Cập nhật trạng thái User nếu được duyệt
                if (dto.IsApproved && request.User != null)
                {
                    request.User.IsVerified = true;
                }

                await _context.SaveChangesAsync();

                // Gửi Email thông báo kết quả
                if (request.User != null && !string.IsNullOrEmpty(request.User.Email))
                {
                    var subject = dto.IsApproved ? "Xác minh tài khoản Sinh viên thành công!" : "Xác minh tài khoản Sinh viên thất bại";
                    
                    var resultMsg = dto.IsApproved 
                        ? "<p>Chúc mừng! Hồ sơ sinh viên của bạn đã được quản trị viên xét duyệt thành công.</p><p>Giờ đây bạn đã có thể đăng nhập và sử dụng toàn bộ tính năng trên hệ sinh thái nhà ở UniNest.</p>" 
                        : $"<p>Rất tiếc, hồ sơ sinh viên của bạn không được phê duyệt.</p><p><b>Lý do: </b> {(string.IsNullOrEmpty(dto.RejectReason) ? "Thẻ sinh viên không hợp lệ hoặc bị mờ." : dto.RejectReason)}</p><p>Vui lòng đăng nhập lại và cập nhật hồ sơ hợp lệ tại hệ thống.</p>";

                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; color: #333;'>
                            <h2 style='color: {(dto.IsApproved ? "#059669" : "#E11D48")}'>Kết quả xác minh UniNest</h2>
                            <p>Chào <b>{request.User.FullName}</b>,</p>
                            {resultMsg}
                            <hr style='border: 1px solid #eee; margin-top: 20px;' />
                            <p style='font-size: 12px; color: #888;'>Cảm ơn bạn đã sử dụng nền tảng của chúng tôi.<br>UniNest Team</p>
                        </div>";

                    await _emailService.SendEmailAsync(request.User.Email, subject, emailBody);
                }

                return Ok(new { message = "Xử lý thành công. Email thông báo đã được gửi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }
    }
}
