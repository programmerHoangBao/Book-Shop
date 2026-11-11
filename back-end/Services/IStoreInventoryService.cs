using back_end.DTOs;
using back_end.DTOs.Stores.Responses;

namespace back_end.Services
{
    public interface IStoreInventoryService
    {
        Task<ApiResponse<PaginationResponse<StoreInventoryBookResponse>>> GetStoreBooksAsync(
            Guid storeId,
            int pageNumber,
            int pageSize);
    }
}
