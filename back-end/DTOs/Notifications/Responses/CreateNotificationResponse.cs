namespace back_end.DTOs.Notifications.Responses
{
    public class CreateNotificationResponse
    {
        public Guid NotificationId { get; set; }
        public string Status { get; set; } = "Queued";
    }
}
