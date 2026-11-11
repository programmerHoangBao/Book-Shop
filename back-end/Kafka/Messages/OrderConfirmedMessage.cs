namespace back_end.Kafka.Messages
{
    public class OrderConfirmedMessage
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime ConfirmedAt { get; set; }
    }
}
