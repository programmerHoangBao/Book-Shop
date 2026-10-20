using back_end.DTOs;
using back_end.DTOs.Users.Requests;

namespace back_end.Services
{
    public interface IUserService
    {
        public Task<ApiResponse<object?>> CreateUserAsync(CreateUserRequest req, IFormFile? file);
    }
}
