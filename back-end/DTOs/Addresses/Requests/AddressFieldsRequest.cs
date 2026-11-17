using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Addresses.Requests
{
    public abstract class AddressFieldsRequest
    {
        [Required]
        [MaxLength(255)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string AddressDetail { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Ward { get; set; }

        [MaxLength(50)]
        public string? District { get; set; }

        [MaxLength(50)]
        public string? Province { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }
    }
}
