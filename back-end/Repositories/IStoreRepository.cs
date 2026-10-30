using back_end.Entities;

namespace back_end.Repositories
{
    public interface IStoreRepository
    {
        Task<bool> AddStoreAsync(StoreEntity store);
        Task<bool> UpdateStoreAsync(StoreEntity store);
        Task<StoreEntity?> GetStoreByIdAsync(Guid storeId);
        Task<(List<StoreEntity> Stores, int TotalItems)> GetStoresAsync(int pageNumber, int pageSize);
        Task<bool> SoftDeleteByIdAsync(Guid storeId);
        Task<(List<StoreEntity> Stores, int TotalItems)> GetDeletedStoresAsync(int pageNumber, int pageSize);
    }
}
