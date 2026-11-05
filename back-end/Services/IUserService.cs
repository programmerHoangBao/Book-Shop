using back_end.DTOs;
using back_end.DTOs.Users.Requests;
using back_end.DTOs.Users.Responses;

namespace back_end.Services
{
    public interface IUserService
    {
        Task<ApiResponse<object?>> CreateUserAsync(CreateUserRequest req);
        Task<ApiResponse<object?>> GetProfileByIdAsync(Guid userId);
        Task<ApiResponse<object?>> GetProfileUserByEmailAsync(string email);
        Task<ApiResponse<object?>> UpdateUserAsync(UpdateUserRequest req);
        Task<ApiResponse<object?>> DeleteUserByIdAsync(Guid userId);
        Task<ApiResponse<object?>> RestoreUserByIdAsync(Guid userId);
        Task<ApiResponse<DeleteUserResponse>> DeleteUsersByListIdAsync(
            DeleteUsersRequest req
        );
        Task<ApiResponse<RestoreUserResponse>> RestoreUserByListIdAsync(
            RestoreUserRequest req
        );
    }
}
