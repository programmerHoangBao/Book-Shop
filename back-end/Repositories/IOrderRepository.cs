using back_end.Entities;
using back_end.DTOs.Orders.Responses;
using back_end.Enums;

namespace back_end.Repositories
{
    public enum PlaceOrderResult
    {
        Created,
        Duplicate,
        AddressNotFound,
        CartEmpty,
        BookUnavailable,
        InsufficientStock,
        InvalidPromotion
    }

    public enum ConfirmOrderResult
    {
        Confirmed,
        AlreadyConfirmed,
        NotFound,
        NotPending,
        StoreNotAssociated
    }

    public enum SetOrderInTransitResult
    {
        InTransit,
        AlreadyInTransit,
        NotFound,
        NotConfirmed,
        StoreNotAssociated
    }

    public enum CompleteOrderResult
    {
        Completed,
        AlreadyCompleted,
        NotFound,
        NotInTransit,
        StoreNotAssociated
    }

    public enum CancelOrderResult
    {
        Cancelled,
        NotFound,
        NotCancellable
    }

    public interface IOrderRepository
    {
        Task<OrderSummaryResponse> GetSummaryAsync(List<Guid>? storeIds);
        Task<(List<OrderEntity> Orders, int TotalItems)> GetInvoicesAsync(
            Guid userId,
            OrderStatus status,
            int pageNumber,
            int pageSize);
        Task<(PlaceOrderResult Result, OrderEntity? Order)> PlaceOrderAsync(
            Guid userId,
            Guid addressId,
            PaymentMethod paymentMethod,
            Guid orderId,
            string? promotionCode);
        Task<(ConfirmOrderResult Result, OrderEntity? Order)> ConfirmAsync(
            Guid orderId,
            Guid? storeId);
        Task<(SetOrderInTransitResult Result, OrderEntity? Order)> SetInTransitAsync(
            Guid orderId,
            Guid? storeId);
        Task<(CompleteOrderResult Result, OrderEntity? Order)> CompleteAsync(
            Guid orderId,
            Guid? storeId);
        Task<(CancelOrderResult Result, DateTime? CancelledAt)> CancelAsync(Guid orderId, Guid userId);
    }
}
