using back_end.DTOs.Reviews.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/reviews")]
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookReviewsAsync(
            [FromQuery] Guid bookId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _reviewService.GetBookReviewsAsync(bookId, pageNumber, pageSize);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpDelete("{reviewId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteReviewAsync(Guid reviewId)
        {
            var response = await _reviewService.DeleteReviewAsync(reviewId);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
