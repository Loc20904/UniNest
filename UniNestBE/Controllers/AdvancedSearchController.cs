using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UniNestBE.DTOs;
using UniNestBE.Entities;
namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvancedSearchController : ControllerBase
    {
        private readonly UniNestDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        // Bổ sung IHttpClientFactory vào Program.cs nếu chưa có (thực ra mặc định .NET 8 có thể inject thẳng hoặc tạo mới)
        // Để đơn giản ta dùng tĩnh cho OSRM public API
        private static readonly HttpClient _httpClient = new HttpClient();

        public AdvancedSearchController(UniNestDbContext context)
        {
            _context = context;
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var universities = await _context.Universities
                .Select(u => new { u.UniId, u.UniName })
                .ToListAsync();

            var propertyTypes = await _context.PropertyTypes
                .Select(p => new { p.PropertyTypeId, p.Name })
                .ToListAsync();

            var amenities = await _context.Amenities
                .Select(a => new { a.AmenityId, a.Name, a.Icon })
                .ToListAsync();

            var lifestyleHabits = await _context.LifestyleHabits
                .Select(h => new { h.LifestyleHabitId, h.Name })
                .ToListAsync();

            return Ok(new
            {
                Universities = universities,
                PropertyTypes = propertyTypes,
                Amenities = amenities,
                LifestyleHabits = lifestyleHabits
            });
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] int universityId = 0,
            [FromQuery] double maxDistanceKm = 50,
            [FromQuery] decimal minPrice = 0,
            [FromQuery] decimal maxPrice = 50000000,
            [FromQuery] string? propertyTypeIds = null,
            [FromQuery] string? amenityIds = null,
            [FromQuery] string? habitIds = null,
            [FromQuery] string? queryStr = null,
            [FromQuery] bool autoMatch = false,
            [FromQuery] int autoMatchUserId = 0)
        {
            var pTypeIds = ParseIds(propertyTypeIds);
            var amIds = ParseIds(amenityIds);
            var hbIds = ParseIds(habitIds);

            List<int> userHabitIds = new List<int>();
            bool isMale = true;
            bool hasProfile = false;

            if (autoMatch && autoMatchUserId > 0)
            {
                var userProfile = await _context.LifestyleProfiles
                    .Include(p => p.User)
                    .Include(p => p.LifestyleHabits)
                    .FirstOrDefaultAsync(p => p.UserId == autoMatchUserId);

                if (userProfile != null)
                {
                    minPrice = userProfile.BudgetMin;
                    maxPrice = userProfile.BudgetMax;
                    userHabitIds = userProfile.LifestyleHabits.Select(h => h.LifestyleHabitId).ToList();
                    hasProfile = true;
                    if (userProfile.User != null) { isMale = userProfile.User.Gender; }
                }
                else
                {
                    return BadRequest("PROFILE_REQUIRED");
                }
            }

            var query = _context.Listings
                .Include(l => l.Address)
                .Include(l => l.Images)
                .Include(l => l.Amenities)
                .Include(l => l.LifestyleHabits)
                .Where(l => l.IsAvailable && l.Address != null && (l.ApprovalStatus == "Approved" || l.ApprovalStatus == "Published") && l.ExpireAt >= DateTime.Now);

            // Filter by Query text
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                var normalQuery = RemoveDiacritics(queryStr);
                query = query.Where(l => (l.Title != null && l.Title.Contains(queryStr)) 
                                      || (l.Title != null && l.Title.Contains(normalQuery))
                                      || (l.Description != null && l.Description.Contains(queryStr))
                                      || (l.Address.District != null && (l.Address.District.Contains(queryStr) || l.Address.District.Contains(normalQuery))));
            }

            // 1. Lọc giá (Bao gồm cả ngầm định từ autoMatch BudgetMax)
            query = query.Where(l => l.Price >= minPrice && l.Price <= maxPrice);

            // 1.5 AutoMatch Hard Filter: Gender
            if (hasProfile)
            {
                var preferredGender = isMale ? "Male" : "Female";
                query = query.Where(l => l.GenderPreference == "Any" || l.GenderPreference == preferredGender);
            }

            // 2. Lọc Property Type
            if (pTypeIds.Any())
            {
                query = query.Where(l => l.PropertyTypeId.HasValue && pTypeIds.Contains(l.PropertyTypeId.Value));
            }

            var listings = await query.ToListAsync();

            if (amIds.Any())
            {
                listings = listings.Where(l => amIds.All(id => l.Amenities.Any(a => a.AmenityId == id))).ToList();
            }

            if (hbIds.Any())
            {
                listings = listings.Where(l => hbIds.All(id => l.LifestyleHabits.Any(h => h.LifestyleHabitId == id))).ToList();
            }

            Address? uniAddress = null;
            if (universityId > 0)
            {
                uniAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.UniversityId == universityId);
            }

            var results = new List<object>();

            foreach (var listing in listings)
            {
                double distanceKm = 0;
                
                // Chỉ tính khoảng cách nếu có chọn universityId ở filter
                if (uniAddress != null)
                {
                    try
                    {
                        var uniLon = uniAddress.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        var uniLat = uniAddress.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        var listLon = listing.Address.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        var listLat = listing.Address.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

                        string osrmUrl = $"http://router.project-osrm.org/route/v1/driving/{uniLon},{uniLat};{listLon},{listLat}?overview=false";
                        
                        var response = await _httpClient.GetAsync(osrmUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonStr = await response.Content.ReadAsStringAsync();
                            using var doc = JsonDocument.Parse(jsonStr);
                            var routes = doc.RootElement.GetProperty("routes");
                            if (routes.GetArrayLength() > 0)
                            {
                                double distanceMeters = routes[0].GetProperty("distance").GetDouble();
                                distanceKm = Math.Round(distanceMeters / 1000.0, 1);
                            }
                        }
                        else
                        {
                            distanceKm = CalculateHaversineDistance((double)uniAddress.Latitude, (double)uniAddress.Longitude, (double)listing.Address.Latitude, (double)listing.Address.Longitude);
                            distanceKm = Math.Round(distanceKm, 1);
                        }
                    }
                    catch
                    {
                        distanceKm = CalculateHaversineDistance((double)uniAddress.Latitude, (double)uniAddress.Longitude, (double)listing.Address.Latitude, (double)listing.Address.Longitude);
                        distanceKm = Math.Round(distanceKm, 1);
                    }
                }

                // Nếu uniAddress != null (chờ filter Distance), và distanceKm > max -> bỏ qua
                if (uniAddress != null && distanceKm > maxDistanceKm) continue;

                int matchPercent = 0;
                
                if (autoMatch && autoMatchUserId > 0)
                {
                    var listingHabitIds = listing.LifestyleHabits.Select(h => h.LifestyleHabitId).ToList();
                    
                    if (listingHabitIds.Any() || userHabitIds.Any())
                    {
                        var intersectCount = listingHabitIds.Intersect(userHabitIds).Count();
                        var maxCount = Math.Max(listingHabitIds.Count( ), userHabitIds.Count);
                        
                        // Công thức: Chống cháy nổ 100% ảo.
                        // Base Point 55% vì đã vượt qua Hard Filter (Tiền, Giới tính, Địa điểm).
                        // 45% còn lại dựa vào tỉ lệ trùng Habit (Jaccard Similarity).
                        double ratio = maxCount == 0 ? 1.0 : (double)intersectCount / maxCount;
                        matchPercent = 55 + (int)Math.Round(ratio * 45);
                    }
                    else
                    {
                        matchPercent = 85; // Hai bên đều không yêu cầu gì -> Khá hợp nhau
                    }

                    // Tạm thời hạ mức lọc xuống 50% thay vì 70% để các phòng cơ sở (55%) vẫn hiện ra
                    if (matchPercent < 50) 
                    {
                        continue; 
                    }
                }
                else
                {
                    matchPercent = 0; // Trạng thái chưa bật matching thì trả về 0
                }
                
                results.Add(new
                {
                    ListingId = listing.ListingId,
                    Title = listing.Title,
                    Price = listing.Price,
                    AreaSquareMeters = listing.AreaSquareMeters,
                    District = listing.Address?.District ?? "Da Nang",
                    DistanceKm = distanceKm,
                    MatchPercent = matchPercent,
                    PrimaryImageUrl = listing.Images?.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() ?? "/images/default.jpg",
                    Amenities = listing.Amenities.Select(a => a.Name).ToList(),
                    CreatedAt = listing.CreatedAt,
                    OwnerId = listing.OwnerId
                });
            }

            if (autoMatch)
            {
                return Ok(results.OrderByDescending(r => ((dynamic)r).MatchPercent));
            }

            return Ok(uniAddress == null ? results.OrderByDescending(r => ((dynamic)r).CreatedAt) : results.OrderBy(r => ((dynamic)r).DistanceKm));
        }

        [HttpGet("distance")]
        public async Task<IActionResult> GetDistance([FromQuery] int listingId, [FromQuery] int universityId)
        {
            var uniAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.UniversityId == universityId);
            var listingAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.ListingId == listingId);

            if (uniAddress == null || listingAddress == null) return BadRequest();

            double distanceKm = 0;
            try
            {
                var uniLon = uniAddress.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var uniLat = uniAddress.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var listLon = listingAddress.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var listLat = listingAddress.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

                string osrmUrl = $"http://router.project-osrm.org/route/v1/driving/{uniLon},{uniLat};{listLon},{listLat}?overview=false";
                var response = await _httpClient.GetAsync(osrmUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonStr);
                    var routes = doc.RootElement.GetProperty("routes");
                    if (routes.GetArrayLength() > 0)
                    {
                        double distanceMeters = routes[0].GetProperty("distance").GetDouble();
                        distanceKm = Math.Round(distanceMeters / 1000.0, 1);
                    }
                }
                else
                {
                    distanceKm = Math.Round(CalculateHaversineDistance((double)uniAddress.Latitude, (double)uniAddress.Longitude, (double)listingAddress.Latitude, (double)listingAddress.Longitude), 1);
                }
            }
            catch
            {
                distanceKm = Math.Round(CalculateHaversineDistance((double)uniAddress.Latitude, (double)uniAddress.Longitude, (double)listingAddress.Latitude, (double)listingAddress.Longitude), 1);
            }

            return Ok(new { distanceKm });
        }

        private List<int> ParseIds(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) return new List<int>();
            return ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(id => int.TryParse(id, out var val) ? val : 0)
                      .Where(val => val > 0)
                      .ToList();
        }

        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1);  // deg2rad below
            var dLon = Deg2Rad(lon2 - lon1);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        private double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180);
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace("đ", "d").Replace("Đ", "D");
        }
    }
}
