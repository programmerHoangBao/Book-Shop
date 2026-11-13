using back_end.DTOs;
using back_end.DTOs.Invoices.Responses;
using back_end.DTOs.Orders.Requests;
using back_end.DTOs.Orders.Responses;
using back_end.Enums;

namespace back_end.Services
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderSummaryResponse>> GetSummaryAsync();
        Task<ApiResponse<PaginationResponse<InvoiceResponse>>> GetInvoicesAsync(
            OrderStatus status,
            int pageNumber,
            int pageSize);
        Task<ApiResponse<object?>> PlaceOrderAsync(
            PlaceOrderRequest request,
            string? idempotencyKey);
        Task<ApiResponse<object?>> ConfirmAsync(Guid orderId);
        Task<ApiResponse<object?>> SetInTransitAsync(Guid orderId);
        Task<ApiResponse<object?>> CompleteAsync(Guid orderId);
        Task<ApiResponse<object?>> CancelAsync(Guid orderId);
    }
}
