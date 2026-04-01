using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniNestBE.DTOs;

namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmenitiesController : ControllerBase
    {
        private readonly UniNestDbContext _context;

        public AmenitiesController(UniNestDbContext context)
        {
            _context = context;
        }

        // GET: api/Amenities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AmenityDto>>> GetAmenities()
        {
            return await _context.Amenities
                .Select(a => new AmenityDto
                {
                    AmenityId = a.AmenityId,
                    Name = a.Name,
                    Icon = a.Icon
                }).ToListAsync();
        }

        // POST: api/Amenities
        [HttpPost]
        [Authorize] // Có thể thêm kiểm tra Role Admin sau này, tạm thời bảo mật bằng Authorize chung
        public async Task<ActionResult<AmenityDto>> CreateAmenity(AmenityDto dto)
        {
            var amenity = new Amenity
            {
                Name = dto.Name,
                Icon = dto.Icon
            };

            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();

            dto.AmenityId = amenity.AmenityId;
            return CreatedAtAction(nameof(GetAmenities), new { id = amenity.AmenityId }, dto);
        }

        // DELETE: api/Amenities/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAmenity(int id)
        {
            var amenity = await _context.Amenities.FindAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }

            _context.Amenities.Remove(amenity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
