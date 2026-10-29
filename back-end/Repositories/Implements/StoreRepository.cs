using back_end.Data;
using back_end.Entities;

namespace back_end.Repositories.Implements
{
    public class StoreRepository : IStoreRepository
    {
        private readonly DatabaseContext _context;

        public StoreRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> AddStoreAsync(StoreEntity store)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Stores.AddAsync(store);
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
