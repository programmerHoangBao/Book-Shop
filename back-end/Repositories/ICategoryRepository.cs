using back_end.Entities;

namespace back_end.Repositories
{
    public interface ICategoryRepository
    {
        Task<bool> CategoryExistsAsync(Guid categoryId);
        Task<bool> CategoryNameExistsAsync(string name, Guid? parentId, Guid? excludeId = null);
        Task<CategoryEntity?> GetCategoryByIdAsync(Guid categoryId);
        Task<bool> AddCategoryAsync(CategoryEntity category);
        Task<bool> UpdateCategoryAsync(CategoryEntity category);
        Task<bool> SoftDeleteCategoryAsync(Guid categoryId);
        Task<bool> RestoreCategoryAsync(Guid categoryId);
        Task<(List<CategoryEntity> Categories, int TotalItems)> GetCategoriesAsync(
            int pageNumber, int pageSize, bool isDeleted = false);
    }
}
