using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/store/{storeId:guid}/books")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class StoreInventoryController : ControllerBase
    {
        private readonly IStoreInventoryService _storeInventoryService;

        public StoreInventoryController(IStoreInventoryService storeInventoryService)
        {
            _storeInventoryService = storeInventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStoreBooksAsync(
            Guid storeId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _storeInventoryService.GetStoreBooksAsync(
                storeId,
                pageNumber,
                pageSize);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
