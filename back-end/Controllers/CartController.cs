using back_end.DTOs.Carts.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddBookAsync([FromBody] AddBookToCartRequest request)
        {
            var response = await _cartService.AddBookAsync(request);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
