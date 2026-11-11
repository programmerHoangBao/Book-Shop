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

    public interface IOrderRepository
    {
        Task<(PlaceOrderResult Result, OrderEntity? Order)> PlaceOrderAsync(
            Guid userId,
            Guid addressId,
            PaymentMethod paymentMethod,
            Guid orderId);
    }
}
