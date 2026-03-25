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
    public class LifestyleHabitsController : ControllerBase
    {
        private readonly UniNestDbContext _context;

        public LifestyleHabitsController(UniNestDbContext context)
        {
            _context = context;
        }

        // GET: api/LifestyleHabits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LifestyleHabitDto>>> GetLifestyleHabits()
        {
            return await _context.LifestyleHabits
                .Select(h => new LifestyleHabitDto
                {
                    LifestyleHabitId = h.LifestyleHabitId,
                    Name = h.Name
                }).ToListAsync();
        }

        // POST: api/LifestyleHabits
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<LifestyleHabitDto>> CreateLifestyleHabit(LifestyleHabitDto dto)
        {
            var habit = new LifestyleHabit
            {
                Name = dto.Name
            };

            _context.LifestyleHabits.Add(habit);
            await _context.SaveChangesAsync();

            dto.LifestyleHabitId = habit.LifestyleHabitId;
            return CreatedAtAction(nameof(GetLifestyleHabits), new { id = habit.LifestyleHabitId }, dto);
        }

        // DELETE: api/LifestyleHabits/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteLifestyleHabit(int id)
        {
            var habit = await _context.LifestyleHabits.FindAsync(id);
            if (habit == null)
            {
                return NotFound();
            }

            _context.LifestyleHabits.Remove(habit);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
