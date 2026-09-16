using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace back_end.Entities
{
    public class OrderItemEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid BookId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal DiscountAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal TotalPrice { get; set; }

        // Navigation properties
        public OrderEntity Order { get; set; } = null!;
        public BookEntity Book { get; set; } = null!;
        public ReturnItemEntity? ReturnItem { get; set; }
    }
}
