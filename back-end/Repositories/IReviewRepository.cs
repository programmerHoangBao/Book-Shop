using back_end.Entities;

namespace back_end.Repositories
{
    public interface IReviewRepository
    {
        Task<bool> HasCompletedPurchaseAsync(Guid userId, Guid bookId, Guid orderId);
        Task<bool> ReviewExistsAsync(Guid userId, Guid bookId, Guid orderId);
        Task<bool> AddReviewAsync(ReviewEntity review);
    }
}
