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
