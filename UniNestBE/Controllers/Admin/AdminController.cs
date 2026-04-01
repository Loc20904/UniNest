using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniNestBE.DTOs.Admin;
using UniNestBE.Entities;
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

        public class ListingActionDto
        {
            public int ListingId { get; set; }
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

                if (!dto.IsApproved && string.IsNullOrWhiteSpace(dto.RejectReason))
                {
                    return BadRequest(new { message = "Vui lòng cung cấp lý do từ chối." });
                }

                var userEmail = request.User?.Email;
                var userFullName = request.User?.FullName;
                var subject = dto.IsApproved ? "Xác minh tài khoản Sinh viên thành công!" : "Xác minh tài khoản Sinh viên thất bại";

                var resultMsg = dto.IsApproved 
                    ? "<p>Chúc mừng! Hồ sơ sinh viên của bạn đã được quản trị viên xét duyệt thành công.</p><p>Giờ đây bạn đã có thể đăng nhập và sử dụng toàn bộ tính năng trên hệ sinh thái nhà ở UniNest.</p>" 
                    : $"<p>Rất tiếc, hồ sơ sinh viên của bạn không được phê duyệt.</p><p><b>Lý do: </b> {dto.RejectReason}</p><p>Hồ sơ của bạn đã bị gắn cờ từ chối. Nếu hồ sơ không được cập nhật lại hoặc vi phạm nặng, tài khoản của bạn sẽ bị hệ thống xóa bỏ.</p>";

                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; color: #333;'>
                        <h2 style='color: {(dto.IsApproved ? "#059669" : "#E11D48")}'>Kết quả xác minh UniNest</h2>
                        <p>Chào <b>{userFullName}</b>,</p>
                        {resultMsg}
                        <hr style='border: 1px solid #eee; margin-top: 20px;' />
                        <p style='font-size: 12px; color: #888;'>Cảm ơn bạn đã sử dụng nền tảng của chúng tôi.<br>UniNest Team</p>
                    </div>";

                // Gửi Email thông báo kết quả trước khi xóa data (nếu từ chối)
                if (!string.IsNullOrEmpty(userEmail))
                {
                    await _emailService.SendEmailAsync(userEmail, subject, emailBody);
                }

                if (dto.IsApproved)
                {
                    request.Status = "Verified";
                    request.ReviewedAt = DateTime.UtcNow;

                    if (request.User != null)
                    {
                        request.User.IsVerified = true;
                    }
                }
                else
                {
                    // Từ chối -> Chuyển Status = Rejected (chưa xóa tài khoản)
                    request.Status = "Rejected";
                    request.ReviewedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = dto.IsApproved ? "Đã duyệt và gửi email thành công." : "Đã từ chối, xóa tài khoản và gửi email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpPost("revert-pending/{requestId}")]
        public async Task<IActionResult> RevertToPending(int requestId)
        {
            try
            {
                var request = await _context.StudentVerificationRequests
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.RequestId == requestId);

                if (request == null) return NotFound(new { message = "Không tìm thấy yêu cầu." });
                if (request.Status == "Pending") return BadRequest(new { message = "Yêu cầu phải đang ở trạng thái Từ chối hoặc Đã duyệt mới có thể hoàn tác." });

                request.Status = "Pending";
                if (request.User != null) request.User.IsVerified = false;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã hoàn tác hồ sơ về trạng thái Chờ duyệt." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpDelete("delete-rejected-account/{requestId}")]
        public async Task<IActionResult> DeleteRejectedAccount(int requestId)
        {
            try
            {
                var request = await _context.StudentVerificationRequests
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.RequestId == requestId);

                if (request == null) return NotFound(new { message = "Không tìm thấy yêu cầu." });
                if (request.Status == "Pending") return BadRequest(new { message = "Chỉ có thể xóa tài khoản khi hồ sơ đã được xử lý (Từ chối hoặc Đã duyệt)." });

                if (request.User != null)
                {
                    _context.StudentVerificationRequests.Remove(request);
                    _context.Users.Remove(request.User);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Tài khoản và hồ sơ vi phạm đã được xóa sổ hoàn toàn khỏi hệ thống." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("student-verifications")]
        public async Task<IActionResult> GetStudentVerifications()
        {
            try
            {
                var verifications = await _context.StudentVerificationRequests
                    .Include(r => r.User)
                    .ThenInclude(u => u.University)
                    .OrderByDescending(r => r.SubmittedAt)
                    .Select(r => new DashboardVerificationRequestDto
                    {
                        RequestId = r.RequestId,
                        StudentName = r.User != null ? (r.User.FullName ?? "N/A") : "N/A",
                        Email = r.User != null ? (r.User.Email ?? "N/A") : "N/A",
                        UniversityName = (r.User != null && r.User.University != null) ? r.User.University.UniName : "N/A",
                        CurrentSemester = r.CurrentSemester ?? "N/A",
                        SubmittedAt = r.SubmittedAt,
                        FrontIdImageUrl = r.FrontIdImageUrl,
                        BackIdImageUrl = r.BackIdImageUrl,
                        Status = r.Status,
                        UserId = r.UserId
                    })
                    .ToListAsync();

                return Ok(verifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalAccounts = await _context.Users.CountAsync();
                var pendingVerifications = await _context.StudentVerificationRequests.CountAsync(r => r.Status == "Pending");
                var pendingListings = await _context.Listings.CountAsync(l => l.ApprovalStatus == "Pending");
                var totalRevenue = 124500000m; // Mockup for now

                var recentVerifications = await _context.StudentVerificationRequests
                    .Include(r => r.User)
                    .ThenInclude(u => u.University)
                    .Where(r => r.Status == "Pending")
                    .OrderByDescending(r => r.SubmittedAt)
                    .Take(5)
                    .Select(r => new DashboardVerificationRequestDto
                    {
                        RequestId = r.RequestId,
                        StudentName = r.User != null ? (r.User.FullName ?? "N/A") : "N/A",
                        Email = r.User != null ? (r.User.Email ?? "N/A") : "N/A",
                        UniversityName = (r.User != null && r.User.University != null) ? r.User.University.UniName : "N/A",
                        CurrentSemester = r.CurrentSemester ?? "N/A",
                        SubmittedAt = r.SubmittedAt,
                        FrontIdImageUrl = r.FrontIdImageUrl,
                        BackIdImageUrl = r.BackIdImageUrl
                    })
                    .ToListAsync();

                var recentListings = await _context.Listings
                    .Include(l => l.Owner)
                    .Include(l => l.Images)
                    .Where(l => l.ApprovalStatus == "Pending")
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(5)
                    .Select(l => new DashboardListingDto
                    {
                        ListingId = l.ListingId,
                        Title = l.Title ?? "N/A",
                        Price = l.Price,
                        OwnerName = l.Owner != null ? (l.Owner.FullName ?? "N/A") : "N/A",
                        OwnerEmail = l.Owner != null ? (l.Owner.Email ?? "N/A") : "N/A",
                        CreatedAt = l.CreatedAt,
                        ImageUrl = l.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? string.Empty,
                        ApprovalStatus = l.ApprovalStatus,
                        IsAvailable = l.IsAvailable,
                        District = l.Address != null ? (l.Address.District ?? "N/A") : "N/A",
                        FullAddress = l.Address != null ? (l.Address.FullAddress ?? "N/A") : "N/A"
                    })
                    .ToListAsync();

                var statsDto = new DashboardStatsDto
                {
                    TotalAccounts = totalAccounts,
                    PendingStudentVerifications = pendingVerifications,
                    PendingListings = pendingListings,
                    TotalRevenue = totalRevenue,
                    RecentVerifications = recentVerifications,
                    RecentListings = recentListings
                };

                return Ok(statsDto);
            }
            catch (Exception ex)
            {
                // return detailed exception
                return StatusCode(500, new { message = $"Đã xảy ra lỗi khi tải thống kê: {ex.ToString()}" });
            }
        }

        [HttpGet("listings")]
        public async Task<IActionResult> GetAdminListings()
        {
            try
            {
                // Prioritize premium listings
                var listings = await _context.Listings
                    .Include(l => l.Owner)
                    .Include(l => l.Address)
                    .Include(l => l.Images)
                    .OrderByDescending(l => l.Owner.IsPremium) // Premium listings first
                    .ThenByDescending(l => l.CreatedAt) // Then by creation date
                    .Select(l => new DashboardListingDto
                    {
                        ListingId = l.ListingId,
                        Title = l.Title ?? "N/A",
                        Price = l.Price,
                        OwnerName = l.Owner != null ? (l.Owner.FullName ?? "N/A") : "N/A",
                        OwnerEmail = l.Owner != null ? (l.Owner.Email ?? "N/A") : "N/A",
                        CreatedAt = l.CreatedAt,
                        ImageUrl = l.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? string.Empty,
                        ApprovalStatus = l.ApprovalStatus,
                        IsAvailable = l.IsAvailable,
                        District = l.Address != null ? (l.Address.District ?? "N/A") : "N/A",
                        FullAddress = l.Address != null ? (l.Address.FullAddress ?? "N/A") : "N/A"
                    })
                    .ToListAsync();

                return Ok(listings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi tải danh sách bài đăng: {ex.Message}" });
            }
        }

        [HttpPost("approve-listing/{id}")]
        public async Task<IActionResult> ApproveListing(int id)
        {
            try
            {
                var listing = await _context.Listings.FindAsync(id);
                if (listing == null) return NotFound(new { message = "Không tìm thấy bài đăng." });

                listing.ApprovalStatus = "Published";
                listing.IsAvailable = true;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã duyệt bài đăng thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost("reject-listing")]
        public async Task<IActionResult> RejectListing([FromBody] ListingActionDto dto)
        {
            try
            {
                var listing = await _context.Listings
                    .Include(l => l.Owner)
                    .FirstOrDefaultAsync(l => l.ListingId == dto.ListingId);

                if (listing == null) return NotFound(new { message = "Không tìm thấy bài đăng." });

                if (string.IsNullOrWhiteSpace(dto.RejectReason))
                {
                    return BadRequest(new { message = "Vui lòng cung cấp lý do từ chối." });
                }

                listing.ApprovalStatus = "Not Approved";
                listing.IsAvailable = false;

                // Send email
                if (listing.Owner != null && !string.IsNullOrEmpty(listing.Owner.Email))
                {
                    var subject = "Thông báo kết quả kiểm duyệt bài đăng - UniNest";
                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; color: #333;'>
                            <h2 style='color: #E11D48'>Bài đăng của bạn không được phê duyệt</h2>
                            <p>Chào <b>{listing.Owner.FullName}</b>,</p>
                            <p>Bài đăng: <b>{listing.Title}</b> đã không vượt qua được quá trình kiểm duyệt.</p>
                            <p><b>Lý do: </b> {dto.RejectReason}</p>
                            <p>Vui lòng cập nhật lại nội dung bài đăng theo đúng quy định của hệ thống.</p>
                            <hr style='border: 1px solid #eee; margin-top: 20px;' />
                            <p style='font-size: 12px; color: #888;'>Cảm ơn bạn đã sử dụng UniNest.<br>UniNest Team</p>
                        </div>";
                    await _emailService.SendEmailAsync(listing.Owner.Email, subject, emailBody);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã từ chối bài đăng và gửi email thông báo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost("block-listing/{id}")]
        public async Task<IActionResult> BlockListing(int id)
        {
            try
            {
                var listing = await _context.Listings.FindAsync(id);
                if (listing == null) return NotFound(new { message = "Không tìm thấy bài đăng." });

                listing.IsAvailable = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã khóa bài đăng." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost("unblock-listing/{id}")]
        public async Task<IActionResult> UnblockListing(int id)
        {
            try
            {
                var listing = await _context.Listings.FindAsync(id);
                if (listing == null) return NotFound(new { message = "Không tìm thấy bài đăng." });

                listing.IsAvailable = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã mở khóa bài đăng." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpDelete("delete-listing/{id}")]
        public async Task<IActionResult> DeleteListing(int id)
        {
            try
            {
                var listing = await _context.Listings.FindAsync(id);
                if (listing == null) return NotFound(new { message = "Không tìm thấy bài đăng." });

                _context.Listings.Remove(listing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã xóa bài đăng vĩnh viễn." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("domains")]
        public async Task<IActionResult> GetDomains()
        {
            var domains = await _context.AllowedEmailDomains.ToListAsync();
            return Ok(domains);
        }

        [HttpPost("domains")]
        public async Task<IActionResult> AddDomain([FromBody] AllowedEmailDomain domain)
        {
            if (string.IsNullOrWhiteSpace(domain.DomainName)) return BadRequest(new { message = "Domain không hợp lệ" });
            _context.AllowedEmailDomains.Add(domain);
            await _context.SaveChangesAsync();
            return Ok(domain);
        }

        [HttpDelete("domains/{id}")]
        public async Task<IActionResult> DeleteDomain(int id)
        {
            var domain = await _context.AllowedEmailDomains.FindAsync(id);
            if (domain == null) return NotFound();
            _context.AllowedEmailDomains.Remove(domain);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
