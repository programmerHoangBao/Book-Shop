namespace back_end.DTOs.Books.Responses
{
    public class AddBookToInventoryResponse
    {
        public Guid BookId { get; set; }
        public Guid InventoryId { get; set; }
        public int QuantityAdded { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public bool BookCreated { get; set; }
    }
}
