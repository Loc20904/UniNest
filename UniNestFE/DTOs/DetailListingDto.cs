using System;
using System.Collections.Generic;

namespace UniNestFE.DTOs
{
    public class DetailListingDto
    {
        public int ListingId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public double AreaSquareMeters { get; set; }
        public string GenderPreference { get; set; }
        public string District { get; set; }
        public string FullAddress { get; set; }
        public string PrimaryImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int OwnerId { get; set; }
        public string HostName { get; set; }
        public string HostAvatar { get; set; }
        public int HostTotalListings { get; set; }

        public List<string> Images { get; set; } = new List<string>();
        public List<AmenityDto> Amenities { get; set; } = new List<AmenityDto>();
        public List<LifestyleHabitDto> LifestyleHabits { get; set; } = new List<LifestyleHabitDto>();
    }

    public class AmenityDto
    {
        public int AmenityId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class LifestyleHabitDto
    {
        public int LifestyleHabitId { get; set; }
        public string Name { get; set; }
    }
}
