using back_end.Data;
using back_end.Entities;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DatabaseContext _context;

        public CategoryRepository(DatabaseContext context)
        {
            _context = context;
        }

        public Task<bool> CategoryExistsAsync(Guid categoryId)
        {
            return _context.Categories.AnyAsync(category =>
                category.Id == categoryId && category.IsActive && !category.IsDeleted);
        }

        public Task<bool> CategoryNameExistsAsync(string name, Guid? parentId, Guid? excludeId = null)
        {
            string normalizedName = name.Trim().ToLower();
            return _context.Categories.AnyAsync(category =>
                !category.IsDeleted && category.ParentId == parentId &&
                category.Name.ToLower() == normalizedName &&
                (!excludeId.HasValue || category.Id != excludeId.Value));
        }

        public Task<CategoryEntity?> GetCategoryByIdAsync(Guid categoryId)
        {
            return _context.Categories.AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == categoryId);
        }

        public async Task<bool> AddCategoryAsync(CategoryEntity category)
        {
            await _context.Categories.AddAsync(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCategoryAsync(CategoryEntity category)
        {
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SoftDeleteCategoryAsync(Guid categoryId)
        {
            int affected = await _context.Categories
                .Where(category => category.Id == categoryId && !category.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(category => category.IsDeleted, true)
                    .SetProperty(category => category.UpdatedAt, DateTime.UtcNow));
            return affected > 0;
        }

        public async Task<bool> RestoreCategoryAsync(Guid categoryId)
        {
            int affected = await _context.Categories
                .Where(category => category.Id == categoryId && category.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(category => category.IsDeleted, false)
                    .SetProperty(category => category.UpdatedAt, DateTime.UtcNow));
            return affected > 0;
        }

        public async Task<(List<CategoryEntity> Categories, int TotalItems)> GetCategoriesAsync(
            int pageNumber, int pageSize, bool isDeleted = false)
        {
            IQueryable<CategoryEntity> query = _context.Categories.AsNoTracking()
                .Where(category => category.IsDeleted == isDeleted);
            int totalItems = await query.CountAsync();
            List<CategoryEntity> categories = await query
                .OrderBy(category => category.Name)
                .ThenBy(category => category.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (categories, totalItems);
        }
    }
}
