using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class PromotionUsageEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid PromotionId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public PromotionEntity? Promotion { get; set; }
        public UserEntity? User { get; set; }
        public OrderEntity? Order { get; set; }
    }
}
