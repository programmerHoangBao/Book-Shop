using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class CategoryEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        public Guid? ParentId { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public CategoryEntity? Parent { get; set; }
        public ICollection<CategoryEntity> Children { get; set; }  = new List<CategoryEntity>();
        public ICollection<BookCategoryEntity> BookCategories { get; set; } = new List<BookCategoryEntity>();
        public ICollection<PromotionCatatogyEntity> PromotionCategories { get; set; } = new List<PromotionCatatogyEntity>();
    }
}
