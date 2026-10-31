using back_end.Data;
using back_end.Entities;

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
    }
}
