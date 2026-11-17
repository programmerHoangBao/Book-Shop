using back_end.DTOs;
using back_end.DTOs.Addresses.Requests;
using back_end.DTOs.Addresses.Responses;

namespace back_end.Services
{
    public interface IAddressService
    {
        Task<ApiResponse<List<AddressResponse>>> GetForCurrentUserAsync();
        Task<ApiResponse<AddressResponse>> GetByIdAsync(Guid addressId);
        Task<ApiResponse<AddressResponse>> CreateAsync(CreateAddressRequest request);
        Task<ApiResponse<AddressResponse>> UpdateAsync(
            Guid addressId,
            UpdateAddressRequest request);
        Task<ApiResponse<AddressResponse>> SetDefaultAsync(Guid addressId);
        Task<ApiResponse<object?>> DeleteAsync(Guid addressId);
    }
}
