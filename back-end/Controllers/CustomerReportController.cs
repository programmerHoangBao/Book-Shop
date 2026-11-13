using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/customer-reports")]
    [Authorize(Roles = "Admin")]
    public class CustomerReportController : ControllerBase
    {
        private readonly ICustomerReportService _service;

        public CustomerReportController(ICustomerReportService service) => _service = service;

        [HttpGet("new-customers")]
        public async Task<IActionResult> GetNewCustomersAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var response = await _service.GetNewCustomersAsync(startDate, endDate);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("returning-customers")]
        public async Task<IActionResult> GetReturningCustomersAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var response = await _service.GetReturningCustomersAsync(startDate, endDate);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
