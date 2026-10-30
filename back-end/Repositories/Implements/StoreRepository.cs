using back_end.Data;
using back_end.Entities;
using Microsoft.EntityFrameworkCore;

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

        public async Task<StoreEntity?> GetStoreByIdAsync(Guid storeId)
        {
            return await _context.Stores.FirstOrDefaultAsync(s => s.Id == storeId);
        }

        public async Task<(List<StoreEntity> Stores, int TotalItems)> GetStoresAsync(
            int pageNumber, 
            int pageSize
        )
        {
            IQueryable<StoreEntity> query = _context.Stores
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt);
            int totalItems = await query.CountAsync();
            List<StoreEntity> stores = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (stores, totalItems);
        }

        public async Task<bool> UpdateStoreAsync(StoreEntity store)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Stores.Update(store);
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
