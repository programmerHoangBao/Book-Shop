using back_end.Data;
using back_end.Entities;
using back_end.Enums;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly DatabaseContext _context;

        public PromotionRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<(bool BookExists, List<PromotionEntity> Promotions, int TotalItems)> GetSuggestionsForBookAsync(
            Guid bookId,
            int pageNumber,
            int pageSize,
            DateTime now)
        {
            bool bookExists = await _context.Books.AnyAsync(book =>
                book.Id == bookId && !book.IsDeleted);
            if (!bookExists)
            {
                return (false, new List<PromotionEntity>(), 0);
            }

            List<Guid> categoryIds = await _context.BookCategories
                .Where(bookCategory => bookCategory.BookId == bookId &&
                    !bookCategory.IsDeleted &&
                    bookCategory.Category != null &&
                    bookCategory.Category.IsActive &&
                    !bookCategory.Category.IsDeleted)
                .Select(bookCategory => bookCategory.CategoryId)
                .ToListAsync();

            IQueryable<PromotionEntity> query = _context.Promotions
                .AsNoTracking()
                .Where(promotion => promotion.IsActive &&
                    !promotion.IsDeleted &&
                    promotion.StartAt <= now &&
                    promotion.EndAt > now &&
                    (promotion.UsageLimit == 0 || promotion.UsageCount < promotion.UsageLimit))
                .Where(promotion =>
                    promotion.PromotionBooks.Any(target =>
                        target.BookId == bookId && !target.IsDeleted) ||
                    promotion.PromotionCatatogies.Any(target =>
                        categoryIds.Contains(target.CategoryId) && !target.IsDeleted) ||
                    (!promotion.PromotionBooks.Any(target => !target.IsDeleted) &&
                     !promotion.PromotionCatatogies.Any(target => !target.IsDeleted)));

            int totalItems = await query.CountAsync();
            List<PromotionEntity> promotions = await query
                .OrderBy(promotion => promotion.PromotionBooks.Any(target =>
                    target.BookId == bookId && !target.IsDeleted) ? 0 :
                    promotion.PromotionCatatogies.Any(target =>
                        categoryIds.Contains(target.CategoryId) && !target.IsDeleted) ? 1 : 2)
                .ThenBy(promotion => promotion.EndAt)
                .ThenBy(promotion => promotion.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (true, promotions, totalItems);
        }

        public Task<bool> CodeExistsAsync(string code) =>
            _context.Promotions.AnyAsync(promotion =>
                promotion.Code != null && promotion.Code.ToLower() == code.ToLower());

        public Task<List<Guid>> GetActiveBookIdsAsync(IReadOnlyCollection<Guid> bookIds) =>
            _context.Books
                .Where(book => bookIds.Contains(book.Id) && !book.IsDeleted && book.Status == BookStatus.Available)
                .Select(book => book.Id)
                .ToListAsync();

        public Task<List<Guid>> GetActiveCategoryIdsAsync(IReadOnlyCollection<Guid> categoryIds) =>
            _context.Categories
                .Where(category => categoryIds.Contains(category.Id) &&
                    category.IsActive && !category.IsDeleted)
                .Select(category => category.Id)
                .ToListAsync();

        public async Task<PromotionEntity> CreateAsync(PromotionEntity promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }
    }
}
