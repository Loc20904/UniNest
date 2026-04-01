using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]

public class MapController : ControllerBase
{
    private readonly UniNestDbContext _context;

    public MapController(UniNestDbContext context)
    {
        _context = context;
    }

    [HttpGet("locations")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLocations()
    {
        var data = await _context.Listings.AsNoTracking()
            // .Include(...) -> KHÔNG CẦN THIẾT khi đã dùng .Select()
            .Where(l => l.IsAvailable && l.Address != null)
            .Select(l => new
            {
                Id = l.ListingId,
                Title = l.Title,
                Price = l.Price,

                // 1. Sửa Warning CS8602: Dùng ! vì ta đã filter Address != null ở trên
                Lat = l.Address!.Latitude,
                Lng = l.Address!.Longitude,
                Address = l.Address!.FullAddress,

                // 2. Sửa lỗi CRASH khi không có ảnh:
                // Cách viết này an toàn: Nếu không tìm thấy ảnh Primary, trả về null, sau đó lấy ảnh mặc định
                Image = l.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() ?? l.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? "default.jpg",

                Gender = l.GenderPreference,

                // 3. Xử lý an toàn cho Owner (đề phòng Listing mất chủ)
                IsVerified = l.Owner != null && l.Owner.IsVerified,
                OwnerID = l.OwnerId
            })
            .ToListAsync();

        return Ok(data);
    }
}