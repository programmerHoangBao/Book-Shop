using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Stores.Requests
{
    public class UpdateStoreRequest
    {
        [Required(ErrorMessage = "Store Id is required!")]
        public Guid StoreId { get; set; }

        [Required(ErrorMessage = "Address is required!")]
        [MaxLength(500, ErrorMessage = "Address length must not exceed 500 characters.")]
        public string Address { get; set; } = string.Empty;
    }
}
