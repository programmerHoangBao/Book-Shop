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

        public async Task<(List<StoreEntity> Stores, int TotalItems)> GetDeletedStoresAsync(
            int pageNumber, 
            int pageSize
        )
        {
            IQueryable<StoreEntity> query = _context.Stores
                .Where(s => s.IsDeleted)
                .OrderByDescending(s => s.UpdatedAt);
            int totalItems = await query.CountAsync();
            List<StoreEntity> stores = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (stores, totalItems);
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

        public async Task<bool> RestoreStoreByIdAsync(Guid storeId)
        {
            var affectedRows = await _context.Stores
                .Where(s => s.Id == storeId && s.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.IsDeleted, false));

            return affectedRows > 0;
        }

        public async Task<(List<StoreEntity> Stores, int TotalItems)> SearchStoresByKeyAndIsDeletedAsync(
            string? key,
            bool isDeleted = true,
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            IQueryable<StoreEntity> query = _context.Stores
                .AsNoTracking()
                .Where(s => s.IsDeleted == isDeleted);

            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Trim();

                query = query.Where(s =>
                    s.Name.Contains(key) ||
                    s.Address.Contains(key) ||
                    s.PhoneNumber.Contains(key)
                );
            }

            int totalItems = await query.CountAsync();

            query = isDeleted
                ? query.OrderByDescending(s => s.UpdatedAt)
                : query.OrderByDescending(s => s.CreatedAt);

            List<StoreEntity> stores = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (stores, totalItems);
        }

        public async Task<bool> SoftDeleteByIdAsync(Guid storeId)
        {
            var affectedRows = await _context.Stores
                .Where(s => s.Id == storeId && !s.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.IsDeleted, true));

            return affectedRows > 0;
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
