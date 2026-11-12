using back_end.Enums;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    [Authorize(Roles = "Customer")]
    public class InvoiceController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public InvoiceController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoicesAsync(
            [FromQuery] OrderStatus status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _orderService.GetInvoicesAsync(status, pageNumber, pageSize);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
