using back_end.Entities;
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
        InsufficientStock
    }

    public enum ConfirmOrderResult
    {
        Confirmed,
        AlreadyConfirmed,
        NotFound,
        NotPending,
        StoreNotAssociated
    }

    public interface IOrderRepository
    {
        Task<(PlaceOrderResult Result, OrderEntity? Order)> PlaceOrderAsync(
            Guid userId,
            Guid addressId,
            PaymentMethod paymentMethod,
            Guid orderId);
        Task<(ConfirmOrderResult Result, OrderEntity? Order)> ConfirmAsync(
            Guid orderId,
            Guid? storeId);
    }
}
