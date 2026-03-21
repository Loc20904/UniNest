using System.ComponentModel.DataAnnotations;

namespace UniNestFE.DTOs
{
    public class EduEmailAttribute : ValidationAttribute
    {
        public EduEmailAttribute()
        {
            ErrorMessage = "Please use a valid university email address ending in .edu.vn.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Return true if null or empty. Use [Required] attribute to enforce presence.
                return true;
            }

            var email = value.ToString()!.Trim().ToLower();
            return email.EndsWith(".edu.vn");
        }
    }
}
