namespace back_end.DTOs.Emails.Requests
{
    public class SendOtpRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public int OtpExpirySeconds { get; set; }
    }
}
