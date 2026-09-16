using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using back_end.Enums;

namespace back_end.Entities
{
    public class BookEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Author { get; set; }
        [MaxLength(255)]
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; } = 0;
        [Required]
        public BookStatus Status { get; set; } = BookStatus.Available;
        public long Rank { get; set; } = -1;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<BookImageEntity> Images { get; set; } = new List<BookImageEntity>();
        public ICollection<BookCategoryEntity> BookCategories { get; set; } = new List<BookCategoryEntity>();
        public ICollection<InventoryEntity> Inventories { get; set; } = new List<InventoryEntity>();
        public ICollection<CartItemEntity> CartItems { get; set; } = new List<CartItemEntity>();
        public ICollection<PromotionBookEntity> PromotionBooks { get; set; } = new List<PromotionBookEntity>();
        public ICollection<OrderItemEntity> OrderItems { get; set; } = new List<OrderItemEntity>();
        public ICollection<ReviewEntity> Reviews { get; set; } = new List<ReviewEntity>();
    }
}
