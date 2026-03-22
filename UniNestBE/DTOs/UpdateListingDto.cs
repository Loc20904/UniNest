namespace UniNestBE.DTOs
{
    public class UpdateListingDto
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public double AreaSquareMeters { get; set; }

        public bool IsAvailable { get; set; }

        public string GenderPreference { get; set; }

        public string District { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public List<int> AmenityIds { get; set; } = new List<int>();

        public List<int> LifestyleHabitIds { get; set; } = new List<int>();
    }
}
