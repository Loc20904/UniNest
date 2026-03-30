using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace UniNestBE.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new UniNestDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<UniNestDbContext>>()))
            {
                // Chạy lệnh này để seed tự động data test vào Database

                // Seed Universities
                if (!context.Universities.Any(u => u.UniName == "Duy Tan University (DTU)"))
                {
                    var unis = new University[]
                    {
                        new University { UniName = "Duy Tan University (DTU)" },
                        new University { UniName = "Da Nang University of Technology" },
                        new University { UniName = "FPT University Da Nang" },
                        new University { UniName = "University of Economics" },
                        new University { UniName = "University of Education" },
                        new University { UniName = "Dong A University" }
                    };
                    context.Universities.AddRange(unis);
                    context.SaveChanges();

                    var uniLocs = new Address[]
                    {
                        new Address { UniversityId = unis[0].UniId, FullAddress = "254 Nguyen Van Linh, Da Nang", Latitude = 16.060132m, Longitude = 108.209678m, District = "Thanh Khe" },
                        new Address { UniversityId = unis[1].UniId, FullAddress = "54 Nguyen Luong Bang, Da Nang", Latitude = 16.073801m, Longitude = 108.149914m, District = "Lien Chieu" },
                        new Address { UniversityId = unis[2].UniId, FullAddress = "Làng Đại học Đà Nẵng, FPT", Latitude = 15.968702m, Longitude = 108.260814m, District = "Ngu Hanh Son" },
                        new Address { UniversityId = unis[3].UniId, FullAddress = "71 Ngu Hanh Son, Da Nang", Latitude = 16.053748m, Longitude = 108.241512m, District = "Ngu Hanh Son" },
                        new Address { UniversityId = unis[4].UniId, FullAddress = "459 Ton Duc Thang, Da Nang", Latitude = 16.061030m, Longitude = 108.156556m, District = "Lien Chieu" },
                        new Address { UniversityId = unis[5].UniId, FullAddress = "33 Xo Viet Nghe Tinh, Da Nang", Latitude = 16.034336m, Longitude = 108.220197m, District = "Hai Chau" }
                    };
                    context.Addresses.AddRange(uniLocs);
                    context.SaveChanges();
                }

                // Seed PropertyTypes
                if (!context.PropertyTypes.Any())
                {
                    context.PropertyTypes.AddRange(
                        new PropertyType { Name = "Studio" },
                        new PropertyType { Name = "Apartment" },
                        new PropertyType { Name = "Shared Room" },
                        new PropertyType { Name = "Entire House" }
                    );
                    context.SaveChanges();
                }

                // Cập nhật listing cũ có propertyTypeId
                var propId = context.PropertyTypes.FirstOrDefault()?.PropertyTypeId;
                if (propId.HasValue)
                {
                    var listingsUpdate = context.Listings.Where(l => l.PropertyTypeId == null).ToList();
                    foreach (var l in listingsUpdate)
                    {
                        l.PropertyTypeId = propId.Value;
                    }
                    context.SaveChanges();
                }

                // Tạo thêm mock listings nếu như chưa đủ 20
                if (context.Listings.Count() < 20)
                {
                    var owner = context.Users.FirstOrDefault();
                    if (owner == null)
                    {
                        owner = new User { FullName = "Admin Default", Email = "admin_mock@test.com", PasswordHash = "123", Role = "admin" };
                        context.Users.Add(owner);
                        context.SaveChanges();
                    }

                    var pStudio = context.PropertyTypes.FirstOrDefault(p => p.Name == "Studio")?.PropertyTypeId ?? propId.Value;
                    var pShared = context.PropertyTypes.FirstOrDefault(p => p.Name == "Shared Room")?.PropertyTypeId ?? propId.Value;
                    var pApartment = context.PropertyTypes.FirstOrDefault(p => p.Name == "Apartment")?.PropertyTypeId ?? propId.Value;

                    string[] titles = { "Cozy Corner", "Urban Retreat", "Student Haven", "Minimalist Flex", "City Lights View", "Sunny Studio", "Quiet Room" };
                    string[] districts = { "Hai Chau", "Thanh Khe", "Cam Le", "Lien Chieu", "Son Tra", "Ngu Hanh Son" };
                    decimal[] lats = { 16.061m, 16.065m, 16.010m, 16.073m, 16.059m, 16.053m };
                    decimal[] lngs = { 108.211m, 108.192m, 108.190m, 108.149m, 108.232m, 108.241m };
                    int[] pTypes = { pStudio, pShared, pApartment };

                    Random random = new Random();
                    
                    var amList = context.Amenities.ToList();
                    var hbList = context.LifestyleHabits.ToList();

                    for (int i = 0; i < 20; i++)
                    {
                        var randDistIdx = random.Next(districts.Length);
                        
                        var randAms = amList.OrderBy(x => Guid.NewGuid()).Take(random.Next(2, 6)).ToList();
                        var randHbs = hbList.OrderBy(x => Guid.NewGuid()).Take(random.Next(1, 4)).ToList();
                        
                        var newListing = new Listing 
                        { 
                            OwnerId = owner.UserId, 
                            Title = $"{titles[random.Next(titles.Length)]} {i+10}", 
                            Price = random.Next(15, 65) * 100000, 
                            AreaSquareMeters = random.Next(15, 50), 
                            PropertyTypeId = pTypes[random.Next(pTypes.Length)],
                            Amenities = randAms,
                            LifestyleHabits = randHbs
                        };
                        
                        context.Listings.Add(newListing);
                        context.SaveChanges();

                        var randLat = lats[randDistIdx] + (decimal)(random.NextDouble() * 0.02 - 0.01);
                        var randLng = lngs[randDistIdx] + (decimal)(random.NextDouble() * 0.02 - 0.01);

                        context.Addresses.Add(new Address 
                        { 
                            ListingId = newListing.ListingId, 
                            FullAddress = $"{random.Next(10, 500)} Random Street, Da Nang", 
                            Latitude = randLat, 
                            Longitude = randLng, 
                            District = districts[randDistIdx] 
                        });

                        context.ListingImages.Add(new ListingImage 
                        { 
                            ListingId = newListing.ListingId, 
                            ImageUrl = $"https://placehold.co/600x400/{(random.Next(100, 999))}/ffffff?text=Room+{i+10}", 
                            IsPrimary = true 
                        });

                        context.SaveChanges();
                    }
                }
            }

            // Seed LifestyleProfile for User 1 (Admin Demo)
            using (var context = new UniNestDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<UniNestDbContext>>()))
            {
                var user1 = context.Users.FirstOrDefault(u => u.UserId == 1);
                if (user1 != null)
                {
                    user1.Gender = true; // Male
                    context.SaveChanges();

                    if (!context.LifestyleProfiles.Any(p => p.UserId == 1))
                    {
                        var hbList = context.LifestyleHabits.ToList();
                        var profile = new LifestyleProfile
                        {
                            UserId = 1,
                            SleepSchedule = "Night Owl",
                            CleanlinessLevel = 4,
                            Smoking = false,
                            HasPet = false,
                            CookingHabit = "Often",
                            BudgetMin = 0,
                            BudgetMax = 4500000,
                            LifestyleHabits = hbList.Take(3).ToList() // Attach first 3 habits
                        };
                        context.LifestyleProfiles.Add(profile);
                        context.SaveChanges();
                    }
                }

                // Removed auto-approve for the first 10 listings so that user's pending items aren't accidentally mutated.
            }
        }
    }
}
