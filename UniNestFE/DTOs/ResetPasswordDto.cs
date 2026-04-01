using System.ComponentModel.DataAnnotations;

namespace UniNestFE.DTOs
{
    public class ResetPasswordDto
    {
        public string? Email { get; set; }
        public string? Token { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "At least 8 characters required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", 
            ErrorMessage = "At least 8 characters, 1 number, and 1 symbol")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmNewPassword { get; set; }
    }
}
