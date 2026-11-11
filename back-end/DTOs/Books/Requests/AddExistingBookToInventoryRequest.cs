using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Books.Requests
{
    public class AddExistingBookToInventoryRequest
    {
        public Guid? StoreId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }
    }
}
