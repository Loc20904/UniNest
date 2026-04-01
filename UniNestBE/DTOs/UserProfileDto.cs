namespace UniNestBE.DTOs
{
    public class UserProfileDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? UniversityName { get; set; }
        public bool IsVerified { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? CurrentAddress { get; set; }
        public string? StudentId { get; set; }
        public string? Major { get; set; }
        public string? YearOfStudy { get; set; }
        public string? EnrollmentStatus { get; set; }
        public bool IsWarned { get; set; }
    }
}
