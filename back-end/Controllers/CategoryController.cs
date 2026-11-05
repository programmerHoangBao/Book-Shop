using back_end.DTOs.Categories.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategoryAsync([FromBody] CreateCategoryRequest request)
        {
            var response = await _categoryService.CreateCategoryAsync(request);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategoryAsync([FromBody] UpdateCategoryRequest request)
        {
            var response = await _categoryService.UpdateCategoryAsync(request);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpDelete("{categoryId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteCategoryAsync(Guid categoryId)
        {
            var response = await _categoryService.SoftDeleteCategoryAsync(categoryId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPatch("{categoryId:guid}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreCategoryAsync(Guid categoryId)
        {
            var response = await _categoryService.RestoreCategoryAsync(categoryId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("{categoryId:guid}")]
        public async Task<IActionResult> GetCategoryByIdAsync(Guid categoryId)
        {
            var response = await _categoryService.GetCategoryByIdAsync(categoryId);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetListCategoryAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _categoryService.GetListCategoryAsync(pageNumber, pageSize);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeletedCategoriesAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _categoryService.GetListCategoryAsync(pageNumber, pageSize, true);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
