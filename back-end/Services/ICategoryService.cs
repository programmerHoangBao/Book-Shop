using back_end.DTOs;
using back_end.DTOs.Categories.Requests;

namespace back_end.Services
{
    public interface ICategoryService
    {
        Task<ApiResponse<object?>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<object?>> UpdateCategoryAsync(UpdateCategoryRequest request);
        Task<ApiResponse<object?>> SoftDeleteCategoryAsync(Guid categoryId);
        Task<ApiResponse<object?>> RestoreCategoryAsync(Guid categoryId);
        Task<ApiResponse<object?>> GetCategoryByIdAsync(Guid categoryId);
        Task<ApiResponse<object?>> GetListCategoryAsync(int pageNumber, int pageSize, bool isDeleted = false);
    }
}
