using back_end.Data;
using back_end.Entities;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class StoreStaffRepository : IStoreStaffRepository
    {
        private readonly DatabaseContext _context;

        public StoreStaffRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> AddStoreStaffAsync(StoreStaffEntity storeStaff)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.StoreStaffs.AddAsync(storeStaff);
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

        public async Task<StoreEntity?> GetStoreByUserIdAsync(Guid userId)
        {
            return await _context.StoreStaffs
                .Where(ss => ss.UserId == userId && !ss.IsDeleted)
                .Select(ss => ss.Store)
                .FirstOrDefaultAsync();
        }

        public Task<bool> IsUserAssignedToStoreAsync(Guid userId, Guid storeId)
        {
            return _context.StoreStaffs.AnyAsync(staff =>
                staff.UserId == userId &&
                staff.StoreId == storeId &&
                !staff.IsDeleted &&
                staff.User != null &&
                !staff.User.IsDeleted &&
                staff.User.Enabled &&
                staff.Store != null &&
                !staff.Store.IsDeleted);
        }

        public async Task<bool> RemoveStoreStaffAsync(StoreStaffEntity storeStaff)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            try
            {
                _context.StoreStaffs.Remove(storeStaff);
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

        public async Task<bool> RestoreByUserIdAsync(Guid userId)
        {
            var affectedRows = await _context.StoreStaffs
                .Where(ss => ss.UserId == userId && ss.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ss => ss.IsDeleted, false));

            return affectedRows > 0;
        }

        public async Task<bool> SoftDeleteByUserIdAsync(Guid userId)
        {
            var affectedRows = await _context.StoreStaffs
                .Where(ss => ss.UserId == userId && !ss.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ss => ss.IsDeleted, true));

            return affectedRows > 0;
        }
    }
}
