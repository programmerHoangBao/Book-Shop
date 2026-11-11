using back_end.DTOs;
using back_end.DTOs.Orders.Requests;

namespace back_end.Services
{
    public interface IOrderService
    {
        Task<ApiResponse<object?>> PlaceOrderAsync(
            PlaceOrderRequest request,
            string? idempotencyKey);
        Task<ApiResponse<object?>> ConfirmAsync(Guid orderId);
    }
}
