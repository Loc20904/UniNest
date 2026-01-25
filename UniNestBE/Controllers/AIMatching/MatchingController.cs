using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Bắt buộc phải đăng nhập mới được tìm ghép đôi
    [Authorize]
    public class MatchingController : ControllerBase
    {
        private readonly IAiMatchingService _matchingService;

        public MatchingController(IAiMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        /// <summary>
        /// Gợi ý người ở ghép dựa trên AI (Lối sống + Vị trí + Trường học)
        /// GET: api/matching/recommendations
        /// </summary>
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            // 1. Lấy UserId từ Token một cách an toàn (Tránh lỗi 500)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin User." });
            }

            try
            {
                // 2. Gọi Service xử lý
                var result = await _matchingService.GetRoommateRecommendations(currentUserId);

                // 3. Trả về kết quả
                // Ngay cả khi không tìm thấy ai (result rỗng), vẫn trả về Ok([]) để Frontend dễ xử lý
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi tại đây (nếu có Logger)
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tính toán gợi ý.", error = ex.Message });
            }
        }
    }
}