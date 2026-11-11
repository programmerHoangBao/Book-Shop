using back_end.DTOs;
using back_end.DTOs.Notifications.Requests;
using back_end.DTOs.Notifications.Responses;

namespace back_end.Services
{
    public interface INotificationService
    {
        Task<ApiResponse<object?>> CreateNotificationAsync(CreateNotificationRequest request);
        Task<ApiResponse<PaginationResponse<NotificationResponse>>> GetForUserAsync(
            Guid userId,
            int pageNumber,
            int pageSize);
    }
}
