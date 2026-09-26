namespace back_end.Redis.Models
{
    public class RefreshTokenModel
    {
        public Guid UserId { get; set; }
        public string RefreshTokenHash { get; set; } = string.Empty;
    }
}
