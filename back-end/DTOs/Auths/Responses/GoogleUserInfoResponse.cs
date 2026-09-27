namespace back_end.DTOs.Auths.Responses
{
    public class GoogleUserInfoResponse
    {
        public string GoogleId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool EmailVerified { get; set; }
    }
}
