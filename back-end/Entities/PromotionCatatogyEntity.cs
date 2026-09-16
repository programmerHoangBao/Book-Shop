using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class PromotionCatatogyEntity : IAuditable
    {
        public Guid PromotionId { get; set; } = Guid.CreateVersion7();
        public Guid CategoryId { get; set; } = Guid.CreateVersion7();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public PromotionEntity? Promotion { get; set; }
        public CategoryEntity? Category { get; set; }
    }
}
