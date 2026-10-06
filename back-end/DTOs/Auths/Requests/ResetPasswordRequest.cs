using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Auths.Requests
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Reset password key is required!")]
        public Guid ResetPasswordKey { get; set; }

        [Required(ErrorMessage = "New password is required!")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])[^\s]{6,15}$",
            ErrorMessage =
                "The password must be between 6 and 15 characters long, " +
                "contain at least one uppercase letter, one lowercase letter, one digit, and one special character, " +
                "and must not contain any spaces."
        )]
        public string NewPassword { get; set; } = string.Empty;
    }
}
