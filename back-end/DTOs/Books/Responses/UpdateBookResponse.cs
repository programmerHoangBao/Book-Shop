namespace back_end.DTOs.Books.Responses
{
    public class UpdateBookResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Guid CategoryId { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
