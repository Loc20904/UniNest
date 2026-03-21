using System.ComponentModel.DataAnnotations;

namespace UniNestFE.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [EduEmail(ErrorMessage = "Password reset requires a valid university email (.edu.vn).")]
        public string Email { get; set; } = string.Empty;
    }
}
