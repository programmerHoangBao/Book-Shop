using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class PromotionBookEntity : IAuditable
    {
        public Guid PromotionId { get; set; } = Guid.CreateVersion7();
        public Guid BookId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public PromotionEntity? Promotion { get; set; }
        public BookEntity? Book { get; set; }
    }
}
