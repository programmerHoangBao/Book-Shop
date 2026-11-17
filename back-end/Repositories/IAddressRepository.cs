using back_end.Entities;

namespace back_end.Repositories
{
    public interface IAddressRepository
    {
        Task<List<AddressEntity>> GetForUserAsync(Guid userId);
        Task<AddressEntity?> GetByIdForUserAsync(Guid userId, Guid addressId);
        Task<AddressEntity> AddForUserAsync(
            Guid userId,
            AddressEntity address,
            bool makeDefault);
        Task<AddressEntity?> UpdateForUserAsync(
            Guid userId,
            Guid addressId,
            AddressEntity updatedAddress);
        Task<AddressEntity?> SetDefaultForUserAsync(Guid userId, Guid addressId);
        Task<bool> SoftDeleteForUserAsync(Guid userId, Guid addressId);
    }
}
