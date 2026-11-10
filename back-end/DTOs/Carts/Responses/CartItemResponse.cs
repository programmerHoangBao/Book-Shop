namespace back_end.DTOs.Carts.Responses
{
    public class CartItemResponse
    {
        public Guid BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
