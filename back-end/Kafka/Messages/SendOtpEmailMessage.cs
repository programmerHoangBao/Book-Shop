namespace back_end.Kafka.Messages
{
    public class SendOtpEmailMessage
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int OtpExpirySeconds { get; set; }
    }
}
