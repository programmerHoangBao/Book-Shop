using back_end.DTOs.Books.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateBookAsync([FromBody] CreateBookRequest request)
        {
            var response = await _bookService.CreateBookAsync(request);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
