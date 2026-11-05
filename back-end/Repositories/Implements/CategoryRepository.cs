
using back_end.Data;
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
    }
}
