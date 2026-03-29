using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniNestBE.Entities;

namespace UniNestBE.Controllers.ManagerListing
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyTypesController : ControllerBase
    {
        private readonly UniNestDbContext _context;

        public PropertyTypesController(UniNestDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PropertyType>>> GetPropertyTypes()
        {
            return await _context.PropertyTypes.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<PropertyType>> PostPropertyType(PropertyType propertyType)
        {
            _context.PropertyTypes.Add(propertyType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPropertyTypes), new { id = propertyType.PropertyTypeId }, propertyType);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePropertyType(int id)
        {
            var propertyType = await _context.PropertyTypes.FindAsync(id);
            if (propertyType == null)
            {
                return NotFound();
            }

            _context.PropertyTypes.Remove(propertyType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
