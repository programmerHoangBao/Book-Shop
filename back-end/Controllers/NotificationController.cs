using back_end.DTOs.Notifications.Requests;
using back_end.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateNotificationAsync(
            [FromBody] CreateNotificationRequest request)
        {
            var response = await _notificationService.CreateNotificationAsync(request);
            return StatusCode(response.HttpStatus, response);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetForUserAsync(
            Guid userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _notificationService.GetForUserAsync(userId, pageNumber, pageSize);
            return StatusCode(response.HttpStatus, response);
        }
    }
}
