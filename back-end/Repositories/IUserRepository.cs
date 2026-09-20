using back_end.Entities;

namespace back_end.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetUserByEmailAsync(string email);
        Task<bool> AddUserAsync(UserEntity user);
    }
}
