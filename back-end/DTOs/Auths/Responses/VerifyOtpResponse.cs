using back_end.Enums;

namespace back_end.DTOs.Auths.Responses
{
    public class VerifyOtpResponse
    {
        public Guid ResetPasswordKey { get; set; }
        public AuthAction Action { get; set; }
    }
}
