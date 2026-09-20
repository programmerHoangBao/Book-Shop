using back_end.DTOs;
using back_end.DTOs.Auths.Requests;

namespace back_end.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<object?>> RegisterAsync(RegisterRequest req);
    }
}
