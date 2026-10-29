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
    }
}
