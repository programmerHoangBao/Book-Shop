namespace back_end.DTOs.Books.Responses
{
    public class CreateBookResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Guid CategoryId { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<CreateBookInventoryResponse> Inventories { get; set; } = new();
    }

    public class CreateBookInventoryResponse
    {
        public Guid StoreId { get; set; }
        public int Quantity { get; set; }
    }
}
