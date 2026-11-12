using back_end.Enums;

namespace back_end.DTOs.Orders.Responses
{
    public class CancelOrderResponse
    {
        public Guid OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime CancelledAt { get; set; }
    }
}
