using back_end.Entities;
using back_end.DTOs.Reviews.Responses;

namespace back_end.Repositories
{
    public interface IReviewRepository
    {
        Task<bool> HasCompletedPurchaseAsync(Guid userId, Guid bookId, Guid orderId);
        Task<bool> ReviewExistsAsync(Guid userId, Guid bookId, Guid orderId);
        Task<bool> AddReviewAsync(ReviewEntity review);
        Task<ReviewEntity?> GetReviewExistByIdAsync(Guid reviewId);
        Task<bool> SoftDeleteReviewAsync(ReviewEntity review);
        Task<bool> BookExistsAsync(Guid bookId);
        Task<(List<ReviewEntity> Reviews, int TotalItems)> GetReviewsAsync(Guid bookId, int pageNumber, int pageSize);
        Task<BookRatingSummaryResponse> GetRatingSummaryAsync(Guid bookId);
    }
}
