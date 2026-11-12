using back_end.Services;
using back_end.DTOs;
using back_end.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/revenue")]
    [Authorize(Roles = "Admin,Manager")]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueReportService _service;

        public RevenueController(IRevenueReportService service) => _service = service;

        /// <summary>Revenue grouped by day, week, month, quarter, or year.</summary>
        [HttpGet("by-period")]
        public async Task<IActionResult> GetByPeriodAsync(
            [FromQuery] string period = "month",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? storeId = null)
        {
            var result = await _service.GetByPeriodAsync(period, startDate, endDate, storeId);
            return StatusCode(200, ApiResponse<RevenueReportResponse>.Response(MessageRecord.Success, data: result));
        }

        /// <summary>Revenue grouped by store.</summary>
        [HttpGet("by-store")]
        public async Task<IActionResult> GetByStoreAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? storeId = null)
        {
            var result = await _service.GetByStoreAsync(startDate, endDate, storeId);
            return StatusCode(200, ApiResponse<RevenueReportResponse>.Response(MessageRecord.Success, data: result));
        }

        /// <summary>Revenue grouped by product.</summary>
        [HttpGet("by-product")]
        public async Task<IActionResult> GetByProductAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? storeId = null)
        {
            var result = await _service.GetByProductAsync(startDate, endDate, storeId);
            return StatusCode(200, ApiResponse<RevenueReportResponse>.Response(MessageRecord.Success, data: result));
        }

        /// <summary>Revenue grouped by category.</summary>
        [HttpGet("by-category")]
        public async Task<IActionResult> GetByCategoryAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? storeId = null)
        {
            var result = await _service.GetByCategoryAsync(startDate, endDate, storeId);
            return StatusCode(200, ApiResponse<RevenueReportResponse>.Response(MessageRecord.Success, data: result));
        }
    }
}
