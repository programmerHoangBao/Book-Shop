using back_end.DTOs.Orders.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PlaceOrderAsync(
            [FromBody] PlaceOrderRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
        {
            var response = await _orderService.PlaceOrderAsync(request, idempotencyKey);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPost("{orderId:guid}/confirm")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> ConfirmOrderAsync(Guid orderId)
        {
            var response = await _orderService.ConfirmAsync(orderId);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
