using back_end.DTOs;
using back_end.DTOs.Auths.Requests;
using back_end.DTOs.Auths.Responses;

namespace back_end.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<object?>> RegisterAsync(RegisterRequest req);
        Task<ApiResponse<LoginResponse?>> LoginAsync(LoginRequest req);
        Task<ApiResponse<object?>> VerifyOtpAsync(VerifyOtpRequest req);
        Task<ApiResponse<object?>> RefreshTokenAsync(RefreshTokenRequest req);
        Task<ApiResponse<LoginResponse?>> GoogleSignInAsync(string idToken);
        Task<ApiResponse<object?>> ForgotPasswordAsync(ForgotPasswordRequest req);
    }
}
