using back_end.DTOs.Auths.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

        [EnableRateLimiting("OtpPolicy")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _authService.RegisterAsync(req);
            return StatusCode(result.HttpStatus, result); // StatusCode is now accessible
        }

        [EnableRateLimiting("LoginPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
        {
            var result = await _authService.LoginAsync(req);
            return StatusCode(result.HttpStatus, result);
        }

        [EnableRateLimiting("OtpPolicy")]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyAsync([FromBody] VerifyOtpRequest req)
        {
            var result = await _authService.VerifyOtpAsync(req);
            return StatusCode(result.HttpStatus, result);
        }

        [EnableRateLimiting("LoginPolicy")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest req)
        {
            var result = await _authService.RefreshTokenAsync(req);
            return StatusCode(result.HttpStatus, result);
        }

        [EnableRateLimiting("LoginPolicy")]
        [HttpPost("google-sign-in")]
        public async Task<IActionResult> GoogleSignInAsync([FromQuery] string idToken)
        {
            var result = await _authService.GoogleSignInAsync(idToken);
            return StatusCode(result.HttpStatus, result);
        }

        [EnableRateLimiting("OtpPolicy")]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest req)
        {
            var result = await _authService.ForgotPasswordAsync(req);
            return StatusCode(result.HttpStatus, result);
        }

        [HttpPatch("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest req)
        {
            var result = await _authService.ResetPasswordAsync(req);
            return StatusCode(result.HttpStatus, result);
        }
    }
}
