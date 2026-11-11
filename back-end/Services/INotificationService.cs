using back_end.DTOs;
using back_end.DTOs.Notifications.Requests;

namespace back_end.Services
{
    public interface INotificationService
    {
        Task<ApiResponse<object?>> CreateNotificationAsync(CreateNotificationRequest request);
    }
}
