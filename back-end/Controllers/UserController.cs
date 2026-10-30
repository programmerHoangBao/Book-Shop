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
    }
}
