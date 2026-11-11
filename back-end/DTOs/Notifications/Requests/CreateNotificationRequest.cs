using back_end.Enums;
using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Notifications.Requests
{
    public class CreateNotificationRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [EnumDataType(typeof(NotificationType))]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [EnumDataType(typeof(NotificationChannel))]
        public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    }
}
