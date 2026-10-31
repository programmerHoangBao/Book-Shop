using back_end.Entities;

namespace back_end.Repositories
{
    public interface IStoreStaffRepository
    {
        Task<bool> AddStoreStaffAsync(StoreStaffEntity storeStaff);
    }
}
