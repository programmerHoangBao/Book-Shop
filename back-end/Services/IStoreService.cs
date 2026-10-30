using back_end.DTOs;
using back_end.DTOs.Stores.Requests;

namespace back_end.Services
{
    public interface IStoreService
    {
        Task<ApiResponse<object?>> CreateStoreAsync(CreateStoreRequest req);
        Task<ApiResponse<object?>> UpdateStoreAsync(UpdateStoreRequest req);
        Task<ApiResponse<object?>> GetStoresAsync(int pageNumber, int pageSize);
        Task<ApiResponse<object?>> GetStoreByIdAsync(Guid storeId);
        Task<ApiResponse<object?>> SoftDeleteByIdAsync(Guid storeId);
    }
}
