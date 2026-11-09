using back_end.Data;
using back_end.DTOs.Reviews.Responses;
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

        public Task<ReviewEntity?> GetReviewExistByIdAsync(Guid reviewId)
        {
            return _context.Reviews.FirstOrDefaultAsync(review =>
                review.Id == reviewId &&
                !review.IsDeleted);
        }

        public async Task<bool> SoftDeleteReviewAsync(ReviewEntity review)
        {
            review.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<ReviewEntity?> GetDeletedReviewByIdAsync(Guid reviewId)
        {
            return _context.Reviews.FirstOrDefaultAsync(review =>
                review.Id == reviewId &&
                review.IsDeleted);
        }

        public async Task<bool> RestoreReviewAsync(ReviewEntity review)
        {
            review.IsDeleted = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<bool> BookExistsAsync(Guid bookId)
        {
            return _context.Books.AnyAsync(book => book.Id == bookId && !book.IsDeleted);
        }

        public async Task<(List<ReviewEntity> Reviews, int TotalItems)> GetReviewsAsync(
            Guid bookId,
            int pageNumber,
            int pageSize)
        {
            IQueryable<ReviewEntity> query = _context.Reviews
                .AsNoTracking()
                .Where(review => review.BookId == bookId && !review.IsDeleted);

            int totalItems = await query.CountAsync();
            List<ReviewEntity> reviews = await query
                .Include(review => review.User)
                .Include(review => review.Images)
                .OrderByDescending(review => review.CreatedAt)
                .ThenByDescending(review => review.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalItems);
        }

        public async Task<BookRatingSummaryResponse> GetRatingSummaryAsync(Guid bookId)
        {
            IQueryable<ReviewEntity> query = _context.Reviews
                .AsNoTracking()
                .Where(review => review.BookId == bookId && !review.IsDeleted);

            int reviewCount = await query.CountAsync();
            double? averageRating = reviewCount == 0
                ? null
                : await query.AverageAsync(review => (double)review.Ratting);

            Dictionary<int, int> countsByRating = await query
                .GroupBy(review => review.Ratting)
                .Select(group => new { Rating = group.Key, Count = group.Count() })
                .ToDictionaryAsync(result => result.Rating, result => result.Count);

            return new BookRatingSummaryResponse
            {
                AverageRating = averageRating,
                ReviewCount = reviewCount,
                RatingDistribution = Enumerable.Range(1, 5)
                    .Select(rating => new RatingDistributionItem
                    {
                        Rating = rating,
                        Count = countsByRating.GetValueOrDefault(rating)
                    })
                    .ToList()
            };
        }
    }
}
