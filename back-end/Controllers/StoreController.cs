using back_end.DTOs.Stores.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/store")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStoreAsync([FromBody] CreateStoreRequest req)
        {
            var response = await _storeService.CreateStoreAsync(req);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStoreAsync([FromBody] UpdateStoreRequest req)
        {
            var response = await _storeService.UpdateStoreAsync(req);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteByIdAsync(
            [FromQuery(Name = "Id")] Guid storeId
        )
        {
            var response = await _storeService.SoftDeleteByIdAsync(storeId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPatch]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreStoreByIdAsync(
            [FromQuery(Name = "Id")] Guid storeId
        )
        {
            var response = await _storeService.RestoreStoreByIdAsync(storeId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetStoresAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var response = await _storeService.GetStoresAsync(pageNumber, pageSize);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("{Id:guid}")]
        public async Task<IActionResult> GetStoreByIdAsync(
            [FromRoute(Name = "Id")] Guid storeId
        )
        {
            var response = await _storeService.GetStoreByIdAsync(storeId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeletedStoresAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var response = await _storeService.GetDeletedStoresAsync(pageNumber, pageSize);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
