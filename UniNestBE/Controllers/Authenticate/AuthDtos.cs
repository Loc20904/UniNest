namespace UniNestBE.DTOs
{
    public class LoginDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class RegisterDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    public class RequestRegistrationDto
    {
        public string? Email { get; set; }
    }

    public class CompleteRegistrationDto
    {
        public string? Token { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public string? Semester { get; set; }
        public IFormFile? frontIdImage { get; set; }
        public IFormFile? backIdImage { get; set; }
    }


    public class ForgotPasswordDto
    {
        public string? Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
    }

    public class AuthResponseDto
    {
        public string? Token { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public int UserId { get; set; }
    }

    public class PremiumSubscriptionDto
    {
        public bool IsPremium { get; set; }
        public DateTime? PremiumExpiryDate { get; set; }
    }
}