using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniNestBE.DTOs;

namespace UniNestBE.Controllers.Authenticate
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UniNestDbContext _context;

        public ProfileController(UniNestDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            var dto = new UserProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender ? "Male" : "Female",
                UniversityName = user.University?.UniName ?? "N/A",
                IsVerified = user.IsVerified,
                ProfileImageUrl = user.StudentAvatar ?? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(user.FullName ?? "User")}&background=38bdf8&color=fff&size=200",
                CurrentAddress = user.CurrentAddress,
                StudentId = user.StudentId,
                Major = user.Major,
                YearOfStudy = user.YearOfStudy,
                EnrollmentStatus = user.EnrollmentStatus
            };

            return Ok(dto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.FullName))
                user.FullName = dto.FullName;
            
            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Gender))
                user.Gender = dto.Gender == "Male";

            if (dto.CurrentAddress != null)
                user.CurrentAddress = dto.CurrentAddress;
            
            if (dto.StudentId != null)
                user.StudentId = dto.StudentId;
            
            if (dto.Major != null)
                user.Major = dto.Major;
            
            if (dto.YearOfStudy != null)
                user.YearOfStudy = dto.YearOfStudy;
            
            if (dto.EnrollmentStatus != null)
                user.EnrollmentStatus = dto.EnrollmentStatus;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpGet("lifestyle")]
        public async Task<IActionResult> GetLifestyleProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var lifestyle = await _context.LifestyleProfiles
                .Include(l => l.LifestyleHabits)
                .FirstOrDefaultAsync(l => l.UserId == userId);
            
            if (lifestyle == null)
            {
                // Return an empty/default complete profile with IsComplete = false
                return Ok(new LifestyleProfileDto { IsComplete = false });
            }

            var dto = new LifestyleProfileDto
            {
                SleepSchedule = lifestyle.SleepSchedule,
                CleanlinessLevel = lifestyle.CleanlinessLevel,
                Smoking = lifestyle.Smoking,
                HasPet = lifestyle.HasPet,
                CookingHabit = lifestyle.CookingHabit,
                GuestFrequency = lifestyle.GuestFrequency,
                PreferredDistricts = lifestyle.PreferredDistricts,
                PersonalityTraits = lifestyle.PersonalityTraits,
                BudgetMin = lifestyle.BudgetMin,
                BudgetMax = lifestyle.BudgetMax,
                IsComplete = true,
                LifestyleHabitIds = lifestyle.LifestyleHabits.Select(h => h.LifestyleHabitId).ToList()
            };

            return Ok(dto);
        }

        [HttpPut("lifestyle")]
        public async Task<IActionResult> UpdateLifestyleProfile([FromBody] LifestyleProfileDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var lifestyle = await _context.LifestyleProfiles
                .Include(l => l.LifestyleHabits)
                .FirstOrDefaultAsync(l => l.UserId == userId);
            if (lifestyle == null)
            {
                lifestyle = new LifestyleProfile { UserId = userId };
                _context.LifestyleProfiles.Add(lifestyle);
            }

            lifestyle.SleepSchedule = dto.SleepSchedule;
            lifestyle.CleanlinessLevel = dto.CleanlinessLevel;
            lifestyle.Smoking = dto.Smoking;
            lifestyle.HasPet = dto.HasPet;
            lifestyle.CookingHabit = dto.CookingHabit;
            lifestyle.GuestFrequency = dto.GuestFrequency;
            lifestyle.PreferredDistricts = dto.PreferredDistricts;
            lifestyle.PersonalityTraits = dto.PersonalityTraits;
            lifestyle.BudgetMin = dto.BudgetMin;
            lifestyle.BudgetMax = dto.BudgetMax;

            lifestyle.LifestyleHabits ??= new List<LifestyleHabit>();
            lifestyle.LifestyleHabits.Clear();

            if (dto.LifestyleHabitIds != null && dto.LifestyleHabitIds.Any())
            {
                var dbHabits = await _context.LifestyleHabits
                    .Where(h => dto.LifestyleHabitIds.Contains(h.LifestyleHabitId))
                    .ToListAsync();
                lifestyle.LifestyleHabits = dbHabits;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Lifestyle profile updated successfully." });
        }
    }
}
