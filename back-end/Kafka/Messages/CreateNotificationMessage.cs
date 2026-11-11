using back_end.Enums;

namespace back_end.Kafka.Messages
{
    public class CreateNotificationMessage
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotificationChannel Channel { get; set; }
    }
}
