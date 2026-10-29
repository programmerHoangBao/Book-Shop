using back_end.Entities;

namespace back_end.Repositories
{
    public interface IStoreRepository
    {
        Task<bool> AddStoreAsync(StoreEntity store);
    }
}
