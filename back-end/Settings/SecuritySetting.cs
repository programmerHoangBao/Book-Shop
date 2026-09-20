namespace back_end.Settings
{
    public class SecuritySetting
    {
        public string SHASecrectKey { get; set; } = string.Empty;
        public string JwtSecretKey { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = string.Empty;
        public string JwtAudience { get; set; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
        public int OtpExpirySeconds { get; set; }
    }
}
