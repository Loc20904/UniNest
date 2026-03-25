namespace UniNestBE.DTOs
{
    public class MyListingDto
    {
        public int ListingId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public double AreaSquareMeters { get; set; }

        public bool IsAvailable { get; set; }
        public string ApprovalStatus { get; set; }

        public string GenderPreference { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public string FullAddress { get; set; }

        public string City { get; set; }

        public string District { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string PrimaryImageUrl { get; set; }
        
        public List<ListingImageDto> Images { get; set; } = new List<ListingImageDto>();

        public List<AmenityDto> Amenities { get; set; } = new List<AmenityDto>();

        public List<LifestyleHabitDto> LifestyleHabits { get; set; } = new List<LifestyleHabitDto>();
    }

    public class ListingImageDto
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
    }
}
