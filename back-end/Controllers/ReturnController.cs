using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/returns")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class ReturnController : ControllerBase
    {
        private readonly IReturnRequestService _returnRequestService;

        public ReturnController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        [HttpPost("{returnRequestId:guid}/confirm")]
        public async Task<IActionResult> ConfirmAsync(Guid returnRequestId)
        {
            var response = await _returnRequestService.ConfirmAsync(returnRequestId);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
