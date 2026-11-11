namespace back_end.Kafka.Messages
{
    public class OrderPlacedMessage
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderedAt { get; set; }
    }
}
