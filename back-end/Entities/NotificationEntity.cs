using back_end.Entities.Interfaces;
using back_end.Enums;
using System.ComponentModel.DataAnnotations;

namespace back_end.Entities
{
    public class NotificationEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content {  get; set; } = string.Empty;

        [Required]
        public NotificationChannel Channel { get; set; }

        [Required]
        public bool IsRead { get; set; } = false;
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public UserEntity? User { get; set; }
    }
}
