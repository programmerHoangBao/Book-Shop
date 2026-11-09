using back_end.Data;
using back_end.Entities;
using back_end.Enums;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DatabaseContext _context;

        public ReviewRepository(DatabaseContext context)
        {
            _context = context;
        }

        public Task<bool> HasCompletedPurchaseAsync(Guid userId, Guid bookId, Guid orderId)
        {
            return _context.Orders.AnyAsync(order =>
                order.Id == orderId &&
                order.UserId == userId &&
                order.OrderStatus == OrderStatus.Completed &&
                !order.IsDeleted &&
                order.Items.Any(item => item.BookId == bookId));
        }

        public Task<bool> ReviewExistsAsync(Guid userId, Guid bookId, Guid orderId)
        {
            return _context.Reviews.AnyAsync(review =>
                review.UserId == userId &&
                review.BookId == bookId &&
                review.OrderId == orderId &&
                !review.IsDeleted);
        }

        public async Task<bool> AddReviewAsync(ReviewEntity review)
        {
            await _context.Reviews.AddAsync(review);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
