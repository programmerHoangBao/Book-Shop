using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Carts.Requests
{
    public class UpdateCartItemQuantityRequest
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
