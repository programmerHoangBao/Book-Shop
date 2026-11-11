using back_end.Data;
using back_end.Entities;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DatabaseContext _context;

        public NotificationRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<(List<NotificationEntity> Notifications, int TotalItems)> GetForUserAsync(
            Guid userId,
            int pageNumber,
            int pageSize)
        {
            IQueryable<NotificationEntity> query = _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.UserId == userId && !notification.IsDeleted);

            int totalItems = await query.CountAsync();
            List<NotificationEntity> notifications = await query
                .OrderByDescending(notification => notification.CreatedAt)
                .ThenByDescending(notification => notification.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalItems);
        }
    }
}
