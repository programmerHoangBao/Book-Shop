using back_end.DTOs.Promotions.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/promotions")]
    [Authorize(Roles = "Admin")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePromotionRequest request)
        {
            var response = await _promotionService.CreateAsync(request);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
