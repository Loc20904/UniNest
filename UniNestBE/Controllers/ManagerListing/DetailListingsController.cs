using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using UniNestBE.DTOs;
using System.Threading.Tasks;

namespace UniNestBE.Controllers.ManagerListing
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetailListingsController : ControllerBase
    {
        private readonly UniNestDbContext _context;

        public DetailListingsController(UniNestDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetailListingDto>> GetDetail(int id)
        {
            var listing = await _context.Listings
                .Include(l => l.Address)
                .Include(l => l.Owner)
                .Include(l => l.Images)
                .Include(l => l.Amenities)
                .Include(l => l.LifestyleHabits)
                .Include(l => l.PropertyType)
                .Where(l => l.ListingId == id && (l.IsAvailable || l.ApprovalStatus == "Pending" || l.ApprovalStatus == "Published"))
                .FirstOrDefaultAsync();

            if (listing == null) return NotFound();

            var totalListingsByHost = await _context.Listings.CountAsync(l => l.OwnerId == listing.OwnerId);

            var dto = new DetailListingDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Price = listing.Price,
                AreaSquareMeters = listing.AreaSquareMeters,
                GenderPreference = listing.GenderPreference,
                District = listing.Address?.District ?? "",
                FullAddress = listing.Address?.FullAddress ?? "",
                PropertyTypeName = listing.PropertyType?.Name ?? "General",
                PrimaryImageUrl = listing.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() ?? "",
                CreatedAt = listing.CreatedAt,
                Description = listing.Description,
                Latitude = (double?)listing.Address?.Latitude ?? 0,
                Longitude = (double?)listing.Address?.Longitude ?? 0,
                OwnerId = listing.OwnerId,
                HostName = listing.Owner?.FullName ?? "Unknown",
                HostAvatar = listing.Owner?.StudentAvatar ?? "default_avatar.jpg",
                HostTotalListings = totalListingsByHost,

                Images = listing.Images.Select(i => i.ImageUrl).ToList(),

                Amenities = listing.Amenities.Select(a => new AmenityDto
                {
                    AmenityId = a.AmenityId,
                    Name = a.Name,
                    Icon = a.Icon
                }).ToList(),

                LifestyleHabits = listing.LifestyleHabits.Select(h => new LifestyleHabitDto
                {
                    LifestyleHabitId = h.LifestyleHabitId,
                    Name = h.Name,
                    Icon = h.Icon
                }).ToList()
            };

            return Ok(dto);
        }
    }
}
