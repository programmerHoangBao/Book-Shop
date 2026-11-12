using back_end.DTOs;
using back_end.DTOs.Returns.Requests;
using back_end.DTOs.Returns.Responses;

namespace back_end.Services
{
    public interface IReturnRequestService
    {
        Task<ApiResponse<PaginationResponse<ReturnRequestResponse>>> GetForCurrentCustomerAsync(
            int pageNumber,
            int pageSize);
        Task<ApiResponse<object?>> CreateAsync(Guid orderId, CreateReturnRequest request);
        Task<ApiResponse<object?>> ConfirmAsync(Guid returnRequestId);
    }
}
