using back_end.DTOs.Auths.Responses;

namespace back_end.Services
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfoResponse?> VerifyTokenAsync(string idToken);
    }
}
