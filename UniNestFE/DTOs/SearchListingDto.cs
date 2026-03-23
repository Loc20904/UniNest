//đây là UC-14: SearchListingDto
using System;

namespace UniNestFE.DTOs
{
    public class SearchListingDto
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
    }
}
//kết thúc UC-14
