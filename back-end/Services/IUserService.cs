using back_end.DTOs;
using back_end.DTOs.Users.Requests;

namespace back_end.Services
{
    public interface IUserService
    {
        public Task<ApiResponse<object?>> CreateUserAsync(CreateUserRequest req, IFormFile? file);
        public Task<ApiResponse<object?>> GetProfileByIdAsync(Guid userId);
        public Task<ApiResponse<object?>> GetProfileUserByEmailAsync(string email);
        public Task<ApiResponse<object?>> UpdateUserAsync(UpdateUserRequest req, IFormFile? file);
    }
}
