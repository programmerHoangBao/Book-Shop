using back_end.DTOs;
using back_end.DTOs.Addresses.Requests;
using back_end.DTOs.Addresses.Responses;
using back_end.Entities;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUserService _currentUserService;

        public AddressService(
            IAddressRepository addressRepository,
            ICurrentUserService currentUserService)
        {
            _addressRepository = addressRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<List<AddressResponse>>> GetForCurrentUserAsync()
        {
            Guid userId = GetCurrentCustomerId();
            List<AddressResponse> addresses = (await _addressRepository.GetForUserAsync(userId))
                .Select(ToResponse)
                .ToList();

            return ApiResponse<List<AddressResponse>>.Response(
                MessageRecord.Success,
                data: addresses);
        }

        public async Task<ApiResponse<AddressResponse>> GetByIdAsync(Guid addressId)
        {
            if (addressId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid userId = GetCurrentCustomerId();
            AddressEntity? address = await _addressRepository.GetByIdForUserAsync(userId, addressId);
            if (address == null)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            return ApiResponse<AddressResponse>.Response(
                MessageRecord.Success,
                data: ToResponse(address));
        }

        public async Task<ApiResponse<AddressResponse>> CreateAsync(CreateAddressRequest request)
        {
            Guid userId = GetCurrentCustomerId();
            AddressEntity address = ToEntity(request);
            AddressEntity createdAddress = await _addressRepository.AddForUserAsync(
                userId,
                address,
                request.IsDefault);

            return ApiResponse<AddressResponse>.Response(
                MessageRecord.CreatedSuccessfully,
                data: ToResponse(createdAddress));
        }

        public async Task<ApiResponse<AddressResponse>> UpdateAsync(
            Guid addressId,
            UpdateAddressRequest request)
        {
            if (addressId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid userId = GetCurrentCustomerId();
            AddressEntity? address = await _addressRepository.UpdateForUserAsync(
                userId,
                addressId,
                ToEntity(request));
            if (address == null)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            return ApiResponse<AddressResponse>.Response(
                MessageRecord.Success,
                data: ToResponse(address));
        }

        public async Task<ApiResponse<AddressResponse>> SetDefaultAsync(Guid addressId)
        {
            if (addressId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid userId = GetCurrentCustomerId();
            AddressEntity? address = await _addressRepository.SetDefaultForUserAsync(userId, addressId);
            if (address == null)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            return ApiResponse<AddressResponse>.Response(
                MessageRecord.Success,
                data: ToResponse(address));
        }

        public async Task<ApiResponse<object?>> DeleteAsync(Guid addressId)
        {
            if (addressId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid userId = GetCurrentCustomerId();
            if (!await _addressRepository.SoftDeleteForUserAsync(userId, addressId))
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            return ApiResponse<object?>.Response(MessageRecord.Success);
        }

        private Guid GetCurrentCustomerId()
        {
            Guid? userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue || userId.Value == Guid.Empty ||
                _currentUserService.GetRoleCurrentUser() != RoleUser.Customer)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            return userId.Value;
        }

        private static AddressEntity ToEntity(AddressFieldsRequest request) => new()
        {
            RecipientName = request.RecipientName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AddressDetail = request.AddressDetail.Trim(),
            Ward = NormalizeOptional(request.Ward),
            District = NormalizeOptional(request.District),
            Province = NormalizeOptional(request.Province),
            City = NormalizeOptional(request.City)
        };

        private static string? NormalizeOptional(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static AddressResponse ToResponse(AddressEntity address) => new()
        {
            Id = address.Id,
            RecipientName = address.RecipientName,
            PhoneNumber = address.PhoneNumber,
            AddressDetail = address.AddressDetail,
            Ward = address.Ward,
            District = address.District,
            Province = address.Province,
            City = address.City,
            IsDefault = address.IsDefault,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }
}
