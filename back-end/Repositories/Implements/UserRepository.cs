using back_end.Data;
using back_end.Entities;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _context;
        public UserRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> AddUserAsync(UserEntity user)
        {
            await using var transaction = 
                await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Users.AddAsync(user);
                var result = await _context.SaveChangesAsync();
                if (result <= 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserEntity?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<UserEntity>> GetUsersByIdsAsync(IReadOnlyCollection<Guid> userIds)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<bool> RestoreUserByIdAsync(Guid userId)
        {
            var affectedRows = await _context.Users
                .Where(u => u.Id == userId && u.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.IsDeleted, false));
            await _context.StoreStaffs
                .Where(ss => ss.UserId == userId && ss.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ss => ss.IsDeleted, false));

            return affectedRows > 0;
        }

        public async Task<List<Guid>> RestoreUserChunkAsync(
            IReadOnlyCollection<Guid> userIds
        )
        {
            if (userIds == null || userIds.Count == 0)
            {
                return new List<Guid>();
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Get valid user IDs
                List<Guid> restoreUserIds = await _context.Users
                    .Where(u =>
                        userIds.Contains(u.Id) &&
                        u.IsDeleted
                    )
                    .Select(u => u.Id)
                    .ToListAsync();

                if (restoreUserIds.Count == 0)
                {
                    await transaction.CommitAsync();
                    return restoreUserIds;
                }

                // Restore users
                await _context.Users
                    .Where(u => restoreUserIds.Contains(u.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(u => u.IsDeleted, false)
                    );

                // Restore related store staff records
                await _context.StoreStaffs
                    .Where(ss =>
                        restoreUserIds.Contains(ss.UserId) &&
                        ss.IsDeleted
                    )
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(ss => ss.IsDeleted, false)
                    );

                await transaction.CommitAsync();

                return restoreUserIds;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> SoftDeleteUserByIdAsync(Guid userId)
        {
            var affectedRows = await _context.Users
                .Where(u => u.Id == userId && !u.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.IsDeleted, true));
            await _context.StoreStaffs
                .Where(ss => ss.UserId == userId && !ss.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ss => ss.IsDeleted, true));

            return affectedRows > 0;
        }

        public async Task<List<Guid>> SoftDeleteUsersChunkAsync(
            IReadOnlyCollection<Guid> userIds
        )
        {
            if (userIds == null || userIds.Count == 0)
            {
                return new List<Guid>();
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Get valid user IDs
                List<Guid> deletedUserIds = await _context.Users
                    .Where(u =>
                        userIds.Contains(u.Id) &&
                        !u.IsDeleted
                    )
                    .Select(u => u.Id)
                    .ToListAsync();

                if (deletedUserIds.Count == 0)
                {
                    await transaction.CommitAsync();
                    return deletedUserIds;
                }

                // Soft delete users
                await _context.Users
                    .Where(u => deletedUserIds.Contains(u.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(u => u.IsDeleted, true)
                    );

                // Soft delete related store staff records
                await _context.StoreStaffs
                    .Where(ss =>
                        deletedUserIds.Contains(ss.UserId) &&
                        !ss.IsDeleted
                    )
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(ss => ss.IsDeleted, true)
                    );

                await transaction.CommitAsync();

                return deletedUserIds;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(UserEntity user)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Update(user);
                var result = await _context.SaveChangesAsync();
                if (result <= 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
