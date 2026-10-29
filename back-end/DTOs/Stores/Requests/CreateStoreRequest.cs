using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Stores.Requests
{
    public class CreateStoreRequest
    {
        [Required(ErrorMessage = "Name is required!")]
        [MaxLength(500, ErrorMessage = "Name length must not exceed 500 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required!")]
        [MaxLength(500, ErrorMessage = "Address length must not exceed 500 characters.")]
        public string Address { get; set; } = string.Empty;


        [MaxLength(11, ErrorMessage = "Phone Number length must not exceed 11 characters.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
