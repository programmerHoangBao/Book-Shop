using back_end.Enums;

namespace back_end.Kafka.Messages
{
    public class PlaceOrderCommandMessage
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? PromotionCode { get; set; }
    }
}
