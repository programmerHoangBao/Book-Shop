using back_end.Entities;

namespace back_end.Repositories
{
    public interface INotificationRepository
    {
        Task<(List<NotificationEntity> Notifications, int TotalItems)> GetForUserAsync(
            Guid userId,
            int pageNumber,
            int pageSize);
    }
}
