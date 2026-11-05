using back_end.Entities;

namespace back_end.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetUserByEmailAsync(string email);
        Task<bool> AddUserAsync(UserEntity user, Guid? storeId = default);
        Task<UserEntity?> GetUserByIdAsync(Guid userId);
        Task<bool> UpdateUserAsync(UserEntity user);
        Task<bool> SoftDeleteUserByIdAsync(Guid userId);
        Task<bool> RestoreUserByIdAsync(Guid userId);
        Task<List<UserEntity>> GetUsersByIdsAsync(
            IReadOnlyCollection<Guid> userIds
        );
        Task<List<Guid>> SoftDeleteUsersChunkAsync(
            IReadOnlyCollection<Guid> userIds
        );

        Task<List<Guid>> RestoreUserChunkAsync(
            IReadOnlyCollection<Guid> userIds
        );
    }
}
