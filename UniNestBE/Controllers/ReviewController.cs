using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly UniNestDbContext _context;

    public ReviewController(UniNestDbContext context)
    {
        _context = context;
    }

    [HttpGet("user/{targetUserId}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<UserReputationDto>> GetUserReputation(int targetUserId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.TargetUserId == targetUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var total = reviews.Count;
        var avg = total > 0 ? reviews.Average(r => r.Rating) : 0;

        var dto = new UserReputationDto
        {
            TargetUserId = targetUserId,
            TotalReviews = total,
            AverageRating = Math.Round(avg, 1),
            RecentReviews = reviews.Take(3).Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.Reviewer?.FullName ?? "Anonymous",
                ReviewerAvatar = r.Reviewer?.StudentAvatar ?? "https://ui-avatars.com/api/?name=User",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost("submit/{targetUserId}")]
    [Authorize]
    public async Task<IActionResult> SubmitReview(int targetUserId, [FromBody] ReviewSubmitDto dto)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized("Token không hợp lệ");
        var reviewerId = int.Parse(claim.Value);

        if (reviewerId == targetUserId)
        {
            return BadRequest(new { message = "You cannot review yourself!" });
        }

        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewerId == reviewerId && r.TargetUserId == targetUserId);

        if (existingReview != null)
        {
            return BadRequest("You have already reviewed this host.");
        }

        // Insert
        var newReview = new Review
        {
            ReviewerId = reviewerId,
            TargetUserId = targetUserId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.Now
        };
        _context.Reviews.Add(newReview);

        await _context.SaveChangesAsync();
        return Ok(new { message = "Review submitted successfully" });
    }
}
