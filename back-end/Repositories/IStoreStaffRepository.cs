using back_end.Entities;

namespace back_end.Repositories
{
    public interface IStoreStaffRepository
    {
        Task<bool> AddStoreStaffAsync(StoreStaffEntity storeStaff);
        Task<bool> RemoveStoreStaffAsync(StoreStaffEntity storeStaff);
        Task<StoreEntity?> GetStoreByUserIdAsync(Guid userId);
        Task<bool> IsUserAssignedToStoreAsync(Guid userId, Guid storeId);
        Task<bool> SoftDeleteByUserIdAsync(Guid userId);
        Task<bool> RestoreByUserIdAsync(Guid userId);
    }
}
