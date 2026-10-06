namespace back_end.Redis.Models
{
    public class ResetPasswordModel
    {
        public Guid ResetPasswordKey { get; set; }
        public Guid UserId { get; set; }
    }
}
