using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Books.Requests
{
    public class CreateBookRequest
    {
        [Required(ErrorMessage = "Book name is required.")]
        [StringLength(500, ErrorMessage = "Book name must not exceed 500 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Author must not exceed 255 characters.")]
        public string? Author { get; set; }

        [StringLength(255, ErrorMessage = "Publisher must not exceed 255 characters.")]
        public string? Publisher { get; set; }

        public string? Description { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Price must be greater than or equal to 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public Guid CategoryId { get; set; }

        public List<string> ImageUrls { get; set; } = new();

        public List<CreateBookInventoryRequest> Inventories { get; set; } = new();
    }

    public class CreateBookInventoryRequest
    {
        [Required(ErrorMessage = "Store ID is required.")]
        public Guid StoreId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Quantity must be greater than or equal to 0."
        )]
        public int Quantity { get; set; } = 0;
    }
}
