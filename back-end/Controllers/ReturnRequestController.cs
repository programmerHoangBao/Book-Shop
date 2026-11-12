using back_end.DTOs.Returns.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/orders/{orderId:guid}/returns")]
    [Authorize(Roles = "Customer")]
    public class ReturnRequestController : ControllerBase
    {
        private readonly IReturnRequestService _returnRequestService;

        public ReturnRequestController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            Guid orderId,
            [FromBody] CreateReturnRequest request)
        {
            var response = await _returnRequestService.CreateAsync(orderId, request);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
