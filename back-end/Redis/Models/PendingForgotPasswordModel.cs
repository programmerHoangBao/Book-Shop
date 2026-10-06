namespace back_end.Redis.Models
{
    public class PendingForgotPasswordModel
    {
        public Guid UserId { get; set; }
        public string Otp { get; set; } = string.Empty;
    }
}
