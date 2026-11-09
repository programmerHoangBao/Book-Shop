namespace back_end.DTOs.Reviews.Responses
{
    public class CreateReviewResponse
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
