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

        [HttpPost("inventory/new")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddNewBookToInventoryAsync(
            [FromBody] AddBookToInventoryRequest request)
        {
            var response = await _bookService.AddNewBookToInventoryAsync(request);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPost("{bookId:guid}/inventory")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddExistingBookToInventoryAsync(
            Guid bookId,
            [FromBody] AddExistingBookToInventoryRequest request)
        {
            var response = await _bookService.AddExistingBookToInventoryAsync(bookId, request);
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

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetDeletedBooksAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _bookService.GetDeletedBooksAsync(pageNumber, pageSize);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("deleted/search")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SearchDeletedBooksAsync(
            [FromQuery] SearchBooksRequest request)
        {
            var response = await _bookService.SearchDeletedBooksAsync(request);
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
