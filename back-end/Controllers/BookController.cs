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

        [HttpPut("{bookId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateBookAsync(
            Guid bookId,
            [FromBody] UpdateBookRequest request)
        {
            var response = await _bookService.UpdateBookAsync(bookId, request);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpDelete("{bookId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteBookAsync(Guid bookId)
        {
            var response = await _bookService.SoftDeleteBookAsync(bookId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPatch("{bookId:guid}/restore")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RestoreBookAsync(Guid bookId)
        {
            var response = await _bookService.RestoreBookAsync(bookId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("{bookId:guid}")]
        public async Task<IActionResult> GetBookDetailsAsync(Guid bookId)
        {
            var response = await _bookService.GetBookDetailsAsync(bookId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetBooksAsync(
            [FromQuery] string? cursor = null,
            [FromQuery] int pageSize = 10)
        {
            var response = await _bookService.GetBooksAsync(cursor, pageSize);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBooksAsync(
            [FromQuery] SearchBooksRequest request)
        {
            var response = await _bookService.SearchBooksAsync(request);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
