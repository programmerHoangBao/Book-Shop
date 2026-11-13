using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/merchandise/reports")]
    [Authorize(Roles = "Admin,Manager")]
    public class MerchandiseReportController : ControllerBase
    {
        private readonly IMerchandiseReportService _service;

        public MerchandiseReportController(IMerchandiseReportService service) => _service = service;

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockAsync([FromQuery] int threshold = 5)
        {
            var response = await _service.GetLowStockAsync(threshold);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("out-of-stock")]
        public async Task<IActionResult> GetOutOfStockAsync()
        {
            var response = await _service.GetOutOfStockAsync();
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("best-selling")]
        public async Task<IActionResult> GetBestSellingAsync(
            [FromQuery] int periodDays = 30,
            [FromQuery] int limit = 10)
        {
            var response = await _service.GetBestSellingAsync(periodDays, limit);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("slow-moving")]
        public async Task<IActionResult> GetSlowMovingAsync(
            [FromQuery] int periodDays = 30,
            [FromQuery] int maximumUnitsSold = 5)
        {
            var response = await _service.GetSlowMovingAsync(periodDays, maximumUnitsSold);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
