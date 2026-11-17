using back_end.DTOs.Addresses.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/user/addresses")]
    [Authorize(Roles = "Customer")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAddressesAsync()
        {
            var response = await _addressService.GetForCurrentUserAsync();
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("{addressId:guid}")]
        public async Task<IActionResult> GetAddressAsync(Guid addressId)
        {
            var response = await _addressService.GetByIdAsync(addressId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddressAsync(
            [FromBody] CreateAddressRequest request)
        {
            var response = await _addressService.CreateAsync(request);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPut("{addressId:guid}")]
        public async Task<IActionResult> UpdateAddressAsync(
            Guid addressId,
            [FromBody] UpdateAddressRequest request)
        {
            var response = await _addressService.UpdateAsync(addressId, request);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPatch("{addressId:guid}/default")]
        public async Task<IActionResult> SetDefaultAddressAsync(Guid addressId)
        {
            var response = await _addressService.SetDefaultAsync(addressId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpDelete("{addressId:guid}")]
        public async Task<IActionResult> DeleteAddressAsync(Guid addressId)
        {
            var response = await _addressService.DeleteAsync(addressId);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
