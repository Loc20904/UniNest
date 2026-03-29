using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using UniNestBE.DTOs;

namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MyListingsController : ControllerBase
    {
        private readonly UniNestDbContext _context;

        public MyListingsController(UniNestDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MyListingDto>>> GetMyListings() //B4: lấy danh sách tin đăng của user để trả về frondend MyListings.razor
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);//B4.1: đọc token rồi chuyển về int để lấy ID //User là một object kiểu ClaimsPrincipal.//Nó đại diện cho người dùng đang đăng nhập vào hệ thống.//Ví dụ khi user login thành công, hệ thống sẽ tạo Claims lưu trong token hoặc cookie.//FindFirstValue(...)lấy giá trị của một Claim.
            var listings = await _context.Listings
                .Where(l => l.OwnerId == userId)// lọc DB
                .Select(l => new MyListingDto
                {
                    ListingId = l.ListingId,
                    Title = l.Title,
                    Description = l.Description,
                    Price = l.Price,
                    AreaSquareMeters = l.AreaSquareMeters,
                    IsAvailable = l.IsAvailable,
                    ApprovalStatus = l.ApprovalStatus,
                    PropertyTypeId = l.PropertyTypeId,
                    GenderPreference = l.GenderPreference,
                    CreatedAt = l.CreatedAt,
                    ExpireAt = l.ExpireAt,
                    FullAddress = _context.Addresses
                        .Where(a => a.AddressId == l.AddressId)
                        .Select(a => a.FullAddress)
                        .FirstOrDefault() ?? "",

                    City = _context.Addresses
                        .Where(a => a.AddressId == l.AddressId)
                        .Select(a => a.City)
                        .FirstOrDefault() ?? "",

                    District = _context.Addresses
                        .Where(a => a.AddressId == l.AddressId)
                        .Select(a => a.District)
                        .FirstOrDefault() ?? "",

                    Amenities = l.Amenities.Select(am => new AmenityDto { AmenityId = am.AmenityId, Name = am.Name, Icon = am.Icon }).ToList(),

                    LifestyleHabits = l.LifestyleHabits.Select(h => new LifestyleHabitDto { LifestyleHabitId = h.LifestyleHabitId, Name = h.Name }).ToList(),

                    PrimaryImageUrl = _context.ListingImages
                        .Where(i => i.ListingId == l.ListingId && i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? "",

                    Images = _context.ListingImages
                        .Where(i => i.ListingId == l.ListingId)
                        .Select(i => new ListingImageDto { ImageId = i.ImageId, ImageUrl = i.ImageUrl, IsPrimary = i.IsPrimary })
                        .ToList()
                })
                .ToListAsync();

            return Ok(listings);//Ok() là một method của ControllerBase. HTTP Status Code: 200 OK
        }

        [HttpPost]
        public async Task<IActionResult> CreateListing(CreateListingDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 1. Tạo mới Address trước
            var address = new Address
            {
                City = "Đà Nẵng",
                District = dto.District,
                FullAddress = dto.Address,
                Latitude = (decimal)dto.Latitude,
                Longitude = (decimal)dto.Longitude
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync(); // Lưu Address để lấy AddressId

            // 2. Lấy danh sách Amenities thực gian từ DB
            var dbAmenities = await _context.Amenities.Where(a => dto.AmenityIds.Contains(a.AmenityId)).ToListAsync();
            var dbHabits = await _context.LifestyleHabits.Where(h => dto.LifestyleHabitIds.Contains(h.LifestyleHabitId)).ToListAsync();

            // 3. Tạo Listing nối với AddressId vừa được sinh
            var listing = new Listing
            {
                OwnerId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                AreaSquareMeters = dto.AreaSquareMeters,
                IsAvailable = dto.IsAvailable,
                ApprovalStatus = !string.IsNullOrEmpty(dto.ApprovalStatus) ? dto.ApprovalStatus : "Pending",
                GenderPreference = dto.GenderPreference,
                Amenities = dbAmenities,
                LifestyleHabits = dbHabits,
                CreatedAt = DateTime.Now,
                ExpireAt = DateTime.Now.AddDays(30),
                AddressId = address.AddressId, // Foreign Key!
                PropertyTypeId = dto.PropertyTypeId
            };

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            // 3. Update lại tham chiếu vòng nếu cần (Tùy chọn)
            address.ListingId = listing.ListingId;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetListing), new { id = listing.ListingId }, new { message = "Listing created successfully", listingId = listing.ListingId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateListing(int id, UpdateListingDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var listing = await _context.Listings
                .Include(l => l.Amenities)
                .Include(l => l.LifestyleHabits)
                .FirstOrDefaultAsync(l => l.ListingId == id && l.OwnerId == userId);

            if (listing == null)
            {
                return NotFound("Listing not found");
            }

            listing.Title = dto.Title;
            listing.Description = dto.Description;
            listing.Price = dto.Price;
            listing.AreaSquareMeters = dto.AreaSquareMeters;
            listing.IsAvailable = dto.IsAvailable;
            listing.GenderPreference = dto.GenderPreference;
            listing.PropertyTypeId = dto.PropertyTypeId;
            if (!string.IsNullOrEmpty(dto.ApprovalStatus))
            {
                listing.ApprovalStatus = dto.ApprovalStatus;
            }

            // Update Amenities Many-to-Many
            listing.Amenities ??= new List<Amenity>();
            listing.Amenities.Clear();
            var dbAmenities = await _context.Amenities.Where(a => dto.AmenityIds.Contains(a.AmenityId)).ToListAsync();
            listing.Amenities = dbAmenities;

            listing.LifestyleHabits ??= new List<LifestyleHabit>();
            listing.LifestyleHabits.Clear();
            var dbHabits = await _context.LifestyleHabits.Where(h => dto.LifestyleHabitIds.Contains(h.LifestyleHabitId)).ToListAsync();
            listing.LifestyleHabits = dbHabits;

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == listing.AddressId);
            bool isNewAddress = false;

            if (address == null)
            {
                address = new Address();
                address.ListingId = listing.ListingId;
                address.City = "Đà Nẵng";
                isNewAddress = true;
            }

            if (!string.IsNullOrEmpty(dto.District)) address.District = dto.District;
            if (!string.IsNullOrEmpty(dto.Address)) address.FullAddress = dto.Address;
            if (dto.Latitude != 0) address.Latitude = (decimal)dto.Latitude;
            if (dto.Longitude != 0) address.Longitude = (decimal)dto.Longitude;

            if (isNewAddress)
            {
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync(); // Save explicitly to retrieve new AddressId
                listing.AddressId = address.AddressId;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Listing updated successfully", listingId = listing.ListingId });
        }

        [HttpPost("{id}/images")]
        public async Task<IActionResult> UploadImages(int id, [FromForm] IFormFileCollection files, [FromForm] string primaryFileName = "")
        {
            if (files == null || files.Count == 0) return BadRequest("No files provided");
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var listing = await _context.Listings.FirstOrDefaultAsync(l => l.ListingId == id && l.OwnerId == userId);
            if (listing == null) return NotFound("Listing not found or access denied");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            bool hasPrimary = await _context.ListingImages.AnyAsync(i => i.ListingId == id && i.IsPrimary);

            var uploadedImages = new List<ListingImageDto>();
            foreach (var file in files)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                bool isThisPrimary = (!hasPrimary) || (file.FileName == primaryFileName);
                if (isThisPrimary)
                {
                    // Xóa primary cũ nếu có
                    var oldPrimary = await _context.ListingImages.FirstOrDefaultAsync(i => i.ListingId == id && i.IsPrimary);
                    if (oldPrimary != null) oldPrimary.IsPrimary = false;
                }

                var img = new ListingImage
                {
                    ListingId = listing.ListingId,
                    ImageUrl = $"/uploads/{fileName}",
                    IsPrimary = isThisPrimary
                };
                if (isThisPrimary) hasPrimary = true;

                _context.ListingImages.Add(img);
                await _context.SaveChangesAsync();

                uploadedImages.Add(new ListingImageDto { ImageId = img.ImageId, ImageUrl = img.ImageUrl, IsPrimary = img.IsPrimary });
            }

            return Ok(uploadedImages);
        }

        [HttpDelete("{id}/images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var listing = await _context.Listings.FirstOrDefaultAsync(l => l.ListingId == id && l.OwnerId == userId);
            if (listing == null) return NotFound("Listing not found or access denied");

            var img = await _context.ListingImages.FirstOrDefaultAsync(i => i.ImageId == imageId && i.ListingId == id);
            if (img == null) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            _context.ListingImages.Remove(img);
            await _context.SaveChangesAsync();

            if (img.IsPrimary)
            {
                var nextImg = await _context.ListingImages.FirstOrDefaultAsync(i => i.ListingId == id);
                if (nextImg != null)
                {
                    nextImg.IsPrimary = true;
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        [HttpPut("{id}/images/{imageId}/primary")]
        public async Task<IActionResult> SetPrimaryImage(int id, int imageId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var listing = await _context.Listings.FirstOrDefaultAsync(l => l.ListingId == id && l.OwnerId == userId);
            if (listing == null) return NotFound("Listing not found or access denied");

            var newPrimary = await _context.ListingImages.FirstOrDefaultAsync(i => i.ImageId == imageId && i.ListingId == id);
            if (newPrimary == null) return NotFound();

            var currentPrimary = await _context.ListingImages.FirstOrDefaultAsync(i => i.ListingId == id && i.IsPrimary);
            if (currentPrimary != null) currentPrimary.IsPrimary = false;

            newPrimary.IsPrimary = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListing(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.ListingId == id && l.OwnerId == userId);

            if (listing == null)
            {
                return NotFound("Listing not found");
            }

            _context.Listings.Remove(listing);

            await _context.SaveChangesAsync();

            return Ok("Listing deleted successfully");
        }

        [HttpPatch("toggle-visibility/{id}")]
        public async Task<IActionResult> ToggleListingVisibility(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.ListingId == id && l.OwnerId == userId);

            if (listing == null)
            {
                return NotFound("Listing not found");
            }

            listing.IsAvailable = !listing.IsAvailable;

            if (!listing.IsAvailable)
            {
                listing.ApprovalStatus = "Hidden";
            }
            else
            {
                listing.ApprovalStatus = "Published";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Listing visibility updated",
                isAvailable = listing.IsAvailable,
                approvalStatus = listing.ApprovalStatus
            });
        }

        [HttpPatch("extend/{id}")]
        public async Task<IActionResult> ExtendListing(int id, ExtendListingDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.ListingId == id && l.OwnerId == userId);

            if (listing == null)
            {
                return NotFound("Listing not found");
            }

            // nếu listing đã hết hạn thì bắt đầu từ hiện tại
            if (listing.ExpireAt < DateTime.Now)
            {
                listing.ExpireAt = DateTime.Now.AddDays(dto.Days);
            }
            else
            {
                listing.ExpireAt = listing.ExpireAt.AddDays(dto.Days);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Listing extended successfully",
                newExpireAt = listing.ExpireAt
            });
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<MyListingDto>> GetListing(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var listing = await _context.Listings
                .Where(l => l.ListingId == id && l.OwnerId == userId)
                .Select(l => new MyListingDto
                {
                    ListingId = l.ListingId,
                    Title = l.Title,
                    Description = l.Description,
                    Price = l.Price,
                    AreaSquareMeters = l.AreaSquareMeters,
                    IsAvailable = l.IsAvailable,
                    ApprovalStatus = l.ApprovalStatus,
                    PropertyTypeId = l.PropertyTypeId,
                    GenderPreference = l.GenderPreference,
                    Amenities = l.Amenities.Select(am => new AmenityDto { AmenityId = am.AmenityId, Name = am.Name, Icon = am.Icon }).ToList(),
                    LifestyleHabits = l.LifestyleHabits.Select(h => new LifestyleHabitDto { LifestyleHabitId = h.LifestyleHabitId, Name = h.Name }).ToList(),
                    CreatedAt = l.CreatedAt,
                    ExpireAt = l.ExpireAt,
                    FullAddress = _context.Addresses.Where(a => a.AddressId == l.AddressId).Select(a => a.FullAddress).FirstOrDefault() ?? "",
                    Address = _context.Addresses.Where(a => a.AddressId == l.AddressId).Select(a => a.FullAddress).FirstOrDefault() ?? "",
                    City = _context.Addresses.Where(a => a.AddressId == l.AddressId).Select(a => a.City).FirstOrDefault() ?? "",
                    District = _context.Addresses.Where(a => a.AddressId == l.AddressId).Select(a => a.District).FirstOrDefault() ?? "",
                    Latitude = _context.Addresses.Where(a => a.AddressId == l.AddressId).Select(a => (double)a.Latitude).FirstOrDefault(),
                    Longitude = _context.Addresses.Where(a => a.AddressId == l.AddressId).Select(a => (double)a.Longitude).FirstOrDefault(),
                    PrimaryImageUrl = _context.ListingImages.Where(i => i.ListingId == l.ListingId && i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() ?? "",
                    Images = _context.ListingImages.Where(i => i.ListingId == l.ListingId).Select(i => new ListingImageDto { ImageId = i.ImageId, ImageUrl = i.ImageUrl, IsPrimary = i.IsPrimary }).ToList()
                })
                .FirstOrDefaultAsync();

            if (listing == null)
                return NotFound();

            return Ok(listing);
        }
    }
}