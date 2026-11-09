using back_end.DTOs;

namespace back_end.DTOs.Reviews.Responses
{
    public class BookReviewsResponse
    {
        public BookRatingSummaryResponse RatingSummary { get; set; } = new();
        public PaginationResponse<BookReviewResponse> Reviews { get; set; } = new();
    }

    public class BookRatingSummaryResponse
    {
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<RatingDistributionItem> RatingDistribution { get; set; } = new();
    }

    public class RatingDistributionItem
    {
        public int Rating { get; set; }
        public int Count { get; set; }
    }

    public class BookReviewResponse
    {
        public Guid Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
