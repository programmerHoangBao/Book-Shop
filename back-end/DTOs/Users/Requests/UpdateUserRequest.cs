using back_end.Enums;
using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Users.Requests
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "User Id is required!")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Full name is required!")]
        [StringLength(255, ErrorMessage = "Full name length must not exceed 255 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        [StringLength(255, ErrorMessage = "Email length must not exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;
    }
}
