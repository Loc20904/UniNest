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
    public async Task<IActionResult> GetLocations()
    {
        // Lấy Listing kèm theo Address và Ảnh đại diện
        var data = await _context.Listings
            .Include(l => l.Address)
            .Include(l => l.Images)
            .Where(l => l.IsAvailable && l.Address != null) // Chỉ lấy phòng còn trống và có địa chỉ
            .Select(l => new
            {
                Id = l.ListingId,
                Title = l.Title,
                Price = l.Price,
                Lat = l.Address.Latitude,
                Lng = l.Address.Longitude,
                Address = l.Address.FullAddress,
                Image = l.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl ?? "default.jpg"
            })
            .ToListAsync();

        return Ok(data);
    }
}