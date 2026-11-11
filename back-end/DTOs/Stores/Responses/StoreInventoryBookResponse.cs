using back_end.Enums;

namespace back_end.DTOs.Stores.Responses
{
    public class StoreInventoryBookResponse
    {
        public Guid BookId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public decimal Price { get; set; }
        public BookStatus Status { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int DamagedQuantity { get; set; }
        public int IncomingQuantity { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
