//đây là UC-14: SearchListingsController (API tìm kiếm)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using UniNestBE.DTOs;

namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchListingsController : ControllerBase
    {
        private readonly UniNestDbContext _context;

        public SearchListingsController(UniNestDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SearchListingDto>>> Search([FromQuery] string query = "", [FromQuery] string district = "")
        {
            
            var queryable = _context.Listings
                .Include(l => l.Address)
                .Include(l => l.Images)
                .Where(l => l.IsAvailable);

            if (!string.IsNullOrEmpty(query))
            {
                queryable = queryable.Where(l => l.Title.Contains(query) || l.Description.Contains(query));
            }

            if (!string.IsNullOrEmpty(district))
            {
                queryable = queryable.Where(l => l.Address != null && l.Address.District != null && l.Address.District.Contains(district));
            }

            var listings = await queryable
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new SearchListingDto
                {
                    ListingId = l.ListingId,
                    Title = l.Title,
                    Price = l.Price,
                    AreaSquareMeters = l.AreaSquareMeters,
                    GenderPreference = l.GenderPreference,
                    District = l.Address != null ? l.Address.District : "",
                    FullAddress = l.Address != null ? l.Address.FullAddress : "",
                    PrimaryImageUrl = l.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() ?? "",
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return Ok(listings);
        }
    }
}
//kết thúc UC-14
