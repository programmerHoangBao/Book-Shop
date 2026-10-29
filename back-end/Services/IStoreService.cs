using back_end.DTOs;
using back_end.DTOs.Stores.Requests;

namespace back_end.Services
{
    public interface IStoreService
    {
        Task<ApiResponse<object?>> CreateStoreAsync(CreateStoreRequest req);
    }
}
