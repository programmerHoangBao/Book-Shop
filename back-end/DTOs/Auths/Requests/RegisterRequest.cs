using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Auths.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full name is required!")]
        [StringLength(255, ErrorMessage = "Full name length must not exceed 255 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        [StringLength(255, ErrorMessage = "Email length must not exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required!")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])[^\s]{6,15}$",
            ErrorMessage =
                "The password must be between 6 and 15 characters long, " +
                "contain at least one uppercase letter, one lowercase letter, one digit, and one special character, " +
                "and must not contain any spaces."
        )]
        public string Password { get; set; } = string.Empty;
    }
}