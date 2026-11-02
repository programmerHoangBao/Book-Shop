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

        public async Task<bool> RestoreUserByIdAsync(Guid userId)
        {
            var affectedRows = await _context.Users
                .Where(u => u.Id == userId && u.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.IsDeleted, false));

            return affectedRows > 0;
        }

        public async Task<bool> SoftDeleteUserByIdAsync(Guid userId)
        {
            var affectedRows = await _context.Users
                .Where(u => u.Id == userId && !u.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.IsDeleted, true));

            return affectedRows > 0;
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
