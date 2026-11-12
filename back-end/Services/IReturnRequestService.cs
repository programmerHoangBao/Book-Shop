using back_end.DTOs;
using back_end.DTOs.Returns.Requests;

namespace back_end.Services
{
    public interface IReturnRequestService
    {
        Task<ApiResponse<object?>> CreateAsync(Guid orderId, CreateReturnRequest request);
        Task<ApiResponse<object?>> ConfirmAsync(Guid returnRequestId);
    }
}
