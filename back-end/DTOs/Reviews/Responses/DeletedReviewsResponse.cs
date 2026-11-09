using back_end.DTOs;

namespace back_end.DTOs.Reviews.Responses
{
    public class DeletedReviewsResponse
    {
        public PaginationResponse<DeletedReviewResponse> Reviews { get; set; } = new();
    }

    public class DeletedReviewResponse
    {
        public Guid ReviewId { get; set; }
        public Guid BookId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
