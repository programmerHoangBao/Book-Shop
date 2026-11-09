using back_end.DTOs.Reviews.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/books/{bookId:guid}/reviews")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReviewAsync(
            [FromBody] CreateReviewRequest request)
        {
            var response = await _reviewService.CreateReviewAsync(request);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
