namespace back_end.DTOs.Users.Infos
{
    public class ErrorDeleteUserInfo
    {
        public Guid UserId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
