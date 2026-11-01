using back_end.DTOs;
using back_end.DTOs.Users.Requests;
using back_end.Services;
using back_end.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Manager,Admin")]
        [Consumes("multipart/form-data")]
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync(
            [FromForm] CreateUserRequest req,
            [FromForm] IFormFile? file = null)
        {
            var response = await _userService.CreateUserAsync(req, file);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProfileByIdAsync([FromQuery(Name = "Id")] Guid userId)
        {
            var response = await _userService.GetProfileByIdAsync(userId);
            return StatusCode(response.HttpStatus, response);
        }

        [Authorize]
        [HttpGet("email")]
        public async Task<IActionResult> GetProfileUserByEmailAsync([FromQuery(Name = "email")] string email)
        {
            var response = await _userService.GetProfileUserByEmailAsync(email);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateUserAsync(
            [FromForm] UpdateUserRequest req,
            [FromForm] IFormFile? file = default)
        {
            var response = await _userService.UpdateUserAsync(req, file);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
