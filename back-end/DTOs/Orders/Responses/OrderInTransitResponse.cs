using back_end.Enums;

namespace back_end.DTOs.Orders.Responses
{
    public class OrderInTransitResponse
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public OrderStatus OrderStatus { get; set; }
        public bool WasAlreadyInTransit { get; set; }
    }
}
