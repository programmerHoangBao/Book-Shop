using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/returns")]
    [Authorize(Roles = "Customer")]
    public class CustomerReturnRequestController : ControllerBase
    {
        private readonly IReturnRequestService _returnRequestService;

        public CustomerReturnRequestController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMineAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _returnRequestService.GetForCurrentCustomerAsync(pageNumber, pageSize);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
