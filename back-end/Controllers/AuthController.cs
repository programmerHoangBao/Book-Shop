using back_end.DTOs.Auths.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _authService.RegisterAsync(req);
            return StatusCode(result.HttpStatus, result); // StatusCode is now accessible
        }
    }
}
