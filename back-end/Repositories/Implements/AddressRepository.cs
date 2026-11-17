using back_end.Data;
using back_end.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace back_end.Repositories.Implements
{
    public class AddressRepository : IAddressRepository
    {
        private readonly DatabaseContext _context;

        public AddressRepository(DatabaseContext context)
        {
            _context = context;
        }

        public Task<List<AddressEntity>> GetForUserAsync(Guid userId)
        {
            return _context.Addresses
                .AsNoTracking()
                .Where(address => address.UserId == userId && !address.IsDeleted)
                .OrderByDescending(address => address.IsDefault)
                .ThenByDescending(address => address.CreatedAt)
                .ToListAsync();
        }

        public Task<AddressEntity?> GetByIdForUserAsync(Guid userId, Guid addressId)
        {
            return _context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(address => address.Id == addressId &&
                    address.UserId == userId &&
                    !address.IsDeleted);
        }

        public async Task<AddressEntity> AddForUserAsync(
            Guid userId,
            AddressEntity address,
            bool makeDefault)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);
            try
            {
                bool hasDefaultAddress = await _context.Addresses.AnyAsync(existing =>
                    existing.UserId == userId &&
                    !existing.IsDeleted &&
                    existing.IsDefault);

                address.UserId = userId;
                address.IsDefault = makeDefault || !hasDefaultAddress;

                if (address.IsDefault)
                {
                    await ClearDefaultAddressAsync(userId);
                }

                await _context.Addresses.AddAsync(address);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return address;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AddressEntity?> UpdateForUserAsync(
            Guid userId,
            Guid addressId,
            AddressEntity updatedAddress)
        {
            AddressEntity? address = await _context.Addresses.FirstOrDefaultAsync(candidate =>
                candidate.Id == addressId &&
                candidate.UserId == userId &&
                !candidate.IsDeleted);

            if (address == null)
            {
                return null;
            }

            address.RecipientName = updatedAddress.RecipientName;
            address.PhoneNumber = updatedAddress.PhoneNumber;
            address.AddressDetail = updatedAddress.AddressDetail;
            address.Ward = updatedAddress.Ward;
            address.District = updatedAddress.District;
            address.Province = updatedAddress.Province;
            address.City = updatedAddress.City;

            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<AddressEntity?> SetDefaultForUserAsync(Guid userId, Guid addressId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);
            try
            {
                bool addressExists = await _context.Addresses.AnyAsync(candidate =>
                    candidate.Id == addressId &&
                    candidate.UserId == userId &&
                    !candidate.IsDeleted);

                if (!addressExists)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await ClearDefaultAddressAsync(userId);
                DateTime updatedAt = DateTime.UtcNow;
                int updatedRows = await _context.Addresses
                    .Where(candidate => candidate.Id == addressId &&
                        candidate.UserId == userId &&
                        !candidate.IsDeleted)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(candidate => candidate.IsDefault, true)
                        .SetProperty(candidate => candidate.UpdatedAt, updatedAt));
                if (updatedRows == 0)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                AddressEntity? address = await _context.Addresses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(candidate => candidate.Id == addressId &&
                        candidate.UserId == userId &&
                        !candidate.IsDeleted);
                await transaction.CommitAsync();
                return address;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> SoftDeleteForUserAsync(Guid userId, Guid addressId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);
            try
            {
                AddressEntity? address = await _context.Addresses.FirstOrDefaultAsync(candidate =>
                    candidate.Id == addressId &&
                    candidate.UserId == userId &&
                    !candidate.IsDeleted);

                if (address == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                AddressEntity? nextDefaultAddress = null;
                if (address.IsDefault)
                {
                    nextDefaultAddress = await _context.Addresses
                        .Where(candidate => candidate.UserId == userId &&
                            candidate.Id != addressId &&
                            !candidate.IsDeleted)
                        .OrderByDescending(candidate => candidate.CreatedAt)
                        .ThenBy(candidate => candidate.Id)
                        .FirstOrDefaultAsync();

                    await ClearDefaultAddressAsync(userId);
                }

                address.IsDeleted = true;
                address.IsDefault = false;
                if (nextDefaultAddress != null)
                {
                    nextDefaultAddress.IsDefault = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private Task<int> ClearDefaultAddressAsync(Guid userId)
        {
            DateTime updatedAt = DateTime.UtcNow;
            return _context.Addresses
                .Where(address => address.UserId == userId &&
                    !address.IsDeleted &&
                    address.IsDefault)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(address => address.IsDefault, false)
                    .SetProperty(address => address.UpdatedAt, updatedAt));
        }
    }
}
