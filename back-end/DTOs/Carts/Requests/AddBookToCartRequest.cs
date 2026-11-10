using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Carts.Requests
{
    public class AddBookToCartRequest
    {
        [Required]
        public Guid BookId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}
