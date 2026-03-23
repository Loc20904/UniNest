using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniNestBE.DTOs;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly UniNestDbContext _context;

    public FavoritesController(UniNestDbContext context)
    {
        _context = context;
    }

    // ✅ Lấy UserId từ JWT
    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // =============================
    // 🔹 1. SAVE LISTING
    // =============================
    [HttpPost("{listingId}")]
    public async Task<IActionResult> SaveListing(int listingId)
    {
        var userId = GetUserId();

        // Check đã save chưa
        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ListingId == listingId);

        if (exists)
            return BadRequest("Already saved");

        var favorite = new Favorite
        {
            UserId = userId,
            ListingId = listingId
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Saved successfully" });
    }

    // =============================
    // 🔹 2. UNSAVE
    // =============================
    [HttpDelete("{listingId}")]
    public async Task<IActionResult> UnsaveListing(int listingId)
    {
        var userId = GetUserId();

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId);

        if (favorite == null)
            return NotFound();

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Removed" });
    }

    // =============================
    // 🔹 3. GET MY FAVORITES
    // =============================
    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = GetUserId();

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Listing)
                .ThenInclude(l => l.Images)
            .Include(f => f.Listing)
                .ThenInclude(l => l.Address)
            .Select(f => new SavedListingDto
            {
                Id = f.Listing.ListingId,
                Title = f.Listing.Title,
                Price = f.Listing.Price,
                District = f.Listing.Address.District,
                ImageUrl = f.Listing.Images.FirstOrDefault().ImageUrl,
                IsBookedOut = !f.Listing.IsAvailable,
                BadgeText = f.Listing.IsAvailable ? "AVAILABLE" : ""
            })
            .ToListAsync();

        return Ok(favorites);
    }

    // =============================
    // 🔹 4. CHECK IS SAVED (optional nhưng cực useful)
    // =============================
    [HttpGet("check/{listingId}")]
    public async Task<IActionResult> IsSaved(int listingId)
    {
        var userId = GetUserId();

        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ListingId == listingId);

        return Ok(exists);
    }

}