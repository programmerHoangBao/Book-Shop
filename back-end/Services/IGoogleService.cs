using back_end.DTOs.Google.Responses;
using back_end.DTOs;

namespace back_end.Services
{
    public interface IGoogleService
    {
        Task<GoogleUserInfoResponse?> VerifyTokenAsync(string idToken);
    }
}
