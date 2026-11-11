using back_end.Enums;

namespace back_end.DTOs.Books.Responses
{
    public class BookDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public BookStatus Status { get; set; }
        public long Rank { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public BookDetailCategoryResponse? Category { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<BookDetailInventoryResponse> Inventories { get; set; } = new();
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class BookDetailCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class BookDetailInventoryResponse
    {
        public Guid StoreId { get; set; }
        public string CodeStore { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
    }
}
