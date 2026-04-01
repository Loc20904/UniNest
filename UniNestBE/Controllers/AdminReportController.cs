using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniNestBE.DTOs;
using UniNestBE.Entities;

namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminReportController : ControllerBase
    {
        private readonly UniNestDbContext _context;

        public AdminReportController(UniNestDbContext context)
        {
            _context = context;
        }

        public class SubmitReportDto
        {
            public int? TargetUserId { get; set; }
            public int? TargetListingId { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string Priority { get; set; } = "Standard";
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitReport([FromBody] SubmitReportDto dto)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int reporterId))
            {
                return Unauthorized(new { message = "You must be logged in to report." });
            }

            // Anti-spam check: one report per target per user
            bool alreadyReported = false;
            
            if (dto.TargetListingId.HasValue) 
            {
                alreadyReported = await _context.Reports.AnyAsync(r => r.ReporterId == reporterId && r.TargetListingId == dto.TargetListingId);
            } 
            else if (dto.TargetUserId.HasValue) 
            {
                alreadyReported = await _context.Reports.AnyAsync(r => r.ReporterId == reporterId && r.TargetUserId == dto.TargetUserId);
            }

            if (alreadyReported)
            {
                return BadRequest(new { message = "Bạn đã báo cáo nội dung này rồi!" });
            }

            // Determine Priority based on Reason
            string priority = "Standard";
            var highPriorityReasons = new[] { "Scam Alert", "Fake Listing", "Harassment", "Lừa đảo", "Bài đăng giả mạo", "Quấy rối" };
            var mediumPriorityReasons = new[] { "Inappropriate Content", "Nội dung phản cảm" };

            if (highPriorityReasons.Contains(dto.Reason))
            {
                priority = "High";
            }
            else if (mediumPriorityReasons.Contains(dto.Reason))
            {
                priority = "Medium";
            }

            var report = new Report
            {
                ReporterId = reporterId,
                TargetUserId = dto.TargetUserId,
                TargetListingId = dto.TargetListingId,
                Reason = dto.Reason,
                Description = dto.Description,
                Priority = priority,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Report submitted successfully." });
        }

        public class ReportDto
        {
            public int ReportId { get; set; }
            public string ReporterName { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public string Priority { get; set; } = "Standard";
            public string Reason { get; set; } = string.Empty;
            public string? Description { get; set; }

            public int? TargetUserId { get; set; }
            public string? TargetUserName { get; set; }
            public string? TargetUserAvatar { get; set; }

            public int? TargetListingId { get; set; }
            public string? TargetListingTitle { get; set; }
            public string? TargetListingOwner { get; set; }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingReports()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.TargetUser)
                .Include(r => r.TargetListing)
                .ThenInclude(l => l.Owner)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.Priority == "High" ? 3 : (r.Priority == "Medium" ? 2 : 1))
                .ThenBy(r => r.CreatedAt)
                .Select(r => new ReportDto
                {
                    ReportId = r.ReportId,
                    ReporterName = r.Reporter != null ? (r.Reporter.FullName ?? "Unknown") : "Unknown",
                    CreatedAt = r.CreatedAt,
                    Priority = r.Priority,
                    Reason = r.Reason,
                    Description = r.Description,
                    
                    TargetUserId = r.TargetUserId,
                    TargetUserName = r.TargetUser != null ? r.TargetUser.FullName : null,
                    TargetUserAvatar = r.TargetUser != null ? r.TargetUser.StudentAvatar : null,

                    TargetListingId = r.TargetListingId,
                    TargetListingTitle = r.TargetListing != null ? r.TargetListing.Title : null,
                    TargetListingOwner = r.TargetListing != null && r.TargetListing.Owner != null ? r.TargetListing.Owner.FullName : null
                })
                .ToListAsync();

            return Ok(reports);
        }

        public class ResolveDto
        {
            public string ActionType { get; set; } = "Dismiss"; // "Dismiss", "Warn", "Ban", "RemovePost"
        }

        [HttpPost("resolve/{reportId}")]
        public async Task<IActionResult> ResolveReport(int reportId, [FromBody] ResolveDto resolveDto)
        {
            var report = await _context.Reports
                .Include(r => r.TargetUser)
                .Include(r => r.TargetListing)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null) return NotFound("Report not found");

            report.Status = "Resolved";
            report.ActionTaken = resolveDto.ActionType;

            switch (resolveDto.ActionType)
            {
                case "Dismiss":
                    report.Status = "Dismissed";
                    break;

                case "Warn":
                    if (report.TargetListingId.HasValue && report.TargetListing != null)
                    {
                        report.TargetListing.WarningCount++;
                        if (report.TargetListing.WarningCount >= 2)
                        {
                            report.TargetListing.ApprovalStatus = "Banned";
                        }
                    }
                    else if (report.TargetUserId.HasValue && report.TargetUser != null)
                    {
                        report.TargetUser.WarningCount++;
                        if (report.TargetUser.WarningCount >= 2)
                        {
                            report.TargetUser.IsBanned = true;
                            // Cascade hide info
                            var userListings = await _context.Listings.Where(l => l.OwnerId == report.TargetUserId).ToListAsync();
                            foreach (var ls in userListings) { ls.ApprovalStatus = "Hidden"; }
                        }
                    }
                    break;

                case "Ban":
                case "BanUser":
                    if (report.TargetUserId.HasValue && report.TargetUser != null)
                    {
                        report.TargetUser.IsBanned = true;
                        var userListings = await _context.Listings.Where(l => l.OwnerId == report.TargetUserId).ToListAsync();
                        foreach (var ls in userListings) { ls.ApprovalStatus = "Hidden"; }
                    }
                    break;

                case "RemovePost":
                    if (report.TargetListingId.HasValue && report.TargetListing != null)
                    {
                        report.TargetListing.ApprovalStatus = "Banned";
                    }
                    break;
            }

            // Mark similar pending reports for this target as resolved
            var otherPending = await _context.Reports
                .Where(r => r.Status == "Pending" && r.ReportId != reportId)
                .Where(r => (report.TargetUserId.HasValue && r.TargetUserId == report.TargetUserId) || 
                            (report.TargetListingId.HasValue && r.TargetListingId == report.TargetListingId))
                .ToListAsync();
            
            foreach(var r in otherPending)
            {
                r.Status = "Resolved";
                r.ActionTaken = resolveDto.ActionType;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Report resolved successfully." });
        }

        // DTO for Lists
        public class PenalizedItemDto
        {
            public string Type { get; set; } = "User"; // "User" or "Listing"
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Warnings { get; set; }
            public string? SubInfo { get; set; }
        }

        [HttpGet("warned")]
        public async Task<IActionResult> GetWarnedList()
        {
            var users = await _context.Users
                .Where(u => u.WarningCount > 0 && !u.IsBanned)
                .Select(u => new PenalizedItemDto
                {
                    Type = "User",
                    Id = u.UserId,
                    Name = u.FullName ?? "Unknown",
                    Warnings = u.WarningCount,
                    SubInfo = u.Email
                }).ToListAsync();

            var listings = await _context.Listings
                .Include(l => l.Owner)
                .Where(l => l.WarningCount > 0 && l.ApprovalStatus != "Banned" && l.ApprovalStatus != "Hidden")
                .Select(l => new PenalizedItemDto
                {
                    Type = "Listing",
                    Id = l.ListingId,
                    Name = l.Title ?? "Unknown",
                    Warnings = l.WarningCount,
                    SubInfo = "Owner: " + l.Owner.FullName
                }).ToListAsync();

            var combined = users.Concat(listings).OrderByDescending(x => x.Warnings).ToList();
            return Ok(combined);
        }

        [HttpGet("banned")]
        public async Task<IActionResult> GetBannedList()
        {
            var users = await _context.Users
                .Where(u => u.IsBanned)
                .Select(u => new PenalizedItemDto
                {
                    Type = "User",
                    Id = u.UserId,
                    Name = u.FullName ?? "Unknown",
                    Warnings = u.WarningCount,
                    SubInfo = u.Email
                }).ToListAsync();

            var listings = await _context.Listings
                .Include(l => l.Owner)
                .Where(l => l.ApprovalStatus == "Banned")
                .Select(l => new PenalizedItemDto
                {
                    Type = "Listing",
                    Id = l.ListingId,
                    Name = l.Title ?? "Unknown",
                    Warnings = l.WarningCount,
                    SubInfo = "Owner: " + l.Owner.FullName
                }).ToListAsync();

            return Ok(users.Concat(listings).ToList());
        }

        [HttpPost("remove-warning")]
        public async Task<IActionResult> RemoveWarning([FromQuery] string type, [FromQuery] int id)
        {
            if (type == "User")
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null) { user.WarningCount = 0; }
            }
            else
            {
                var listing = await _context.Listings.FindAsync(id);
                if (listing != null) { listing.WarningCount = 0; }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("ban-direct")]
        public async Task<IActionResult> BanDirect([FromQuery] string type, [FromQuery] int id)
        {
            if (type == "User")
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null) 
                { 
                    user.IsBanned = true; 
                    var listings = await _context.Listings.Where(l => l.OwnerId == id).ToListAsync();
                    foreach (var l in listings) { l.ApprovalStatus = "Hidden"; }
                }
            }
            else
            {
                var listing = await _context.Listings.FindAsync(id);
                if (listing != null) { listing.ApprovalStatus = "Banned"; }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("unban")]
        public async Task<IActionResult> Unban([FromQuery] string type, [FromQuery] int id)
        {
            if (type == "User")
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null) 
                { 
                    user.IsBanned = false; 
                    user.WarningCount = 0; 
                    // Bỏ ẩn bài (Giả định về Published)
                    var listings = await _context.Listings.Where(l => l.OwnerId == id && l.ApprovalStatus == "Hidden").ToListAsync();
                    foreach (var l in listings) { l.ApprovalStatus = "Published"; }
                }
            }
            else
            {
                var listing = await _context.Listings.FindAsync(id);
                if (listing != null) 
                { 
                    listing.ApprovalStatus = "Published"; 
                    listing.WarningCount = 0;
                }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("delete-permanently")]
        public async Task<IActionResult> DeletePermanently([FromQuery] string type, [FromQuery] int id)
        {
            if (type == "User")
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null) 
                { 
                    var listings = await _context.Listings.Where(l => l.OwnerId == id).ToListAsync();
                    _context.Listings.RemoveRange(listings); // Xóa sổ
                    _context.Users.Remove(user);
                }
            }
            else
            {
                var listing = await _context.Listings.FindAsync(id);
                if (listing != null) 
                { 
                    _context.Listings.Remove(listing); 
                }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
