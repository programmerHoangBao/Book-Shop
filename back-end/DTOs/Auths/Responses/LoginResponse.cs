namespace back_end.DTOs.Auths.Responses
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
