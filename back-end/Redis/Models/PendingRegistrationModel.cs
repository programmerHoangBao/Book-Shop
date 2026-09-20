namespace back_end.Redis.Models
{
    public class PendingRegistrationModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
