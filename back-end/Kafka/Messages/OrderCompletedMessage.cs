using back_end.Enums;

namespace back_end.Kafka.Messages
{
    public class OrderCompletedMessage
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
