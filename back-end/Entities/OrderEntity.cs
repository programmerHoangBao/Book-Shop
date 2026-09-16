using back_end.Entities.Interfaces;
using back_end.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class OrderEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid AddressId {  get; set; }

        [Required]
        [MaxLength(6)]
        [Column(TypeName = "varchar(6)")]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal DiscountAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal ShippingFee { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal TotalAmount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        [Required]
        public PaymentStatus PaymentStatus { get; set; }

        [Required]
        public OrderStatus OrderStatus { get; set; }

        [Required]
        public DateTime OrderedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public UserEntity? User { get; set; }
        public AddressEntity? Address { get; set; }
        public ICollection<PromotionUsageEntity> PromotionUsages { get; set; } = new List<PromotionUsageEntity>();
        public ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
        public ICollection<ReviewEntity> Reviews { get; set; } = new List<ReviewEntity>();
        public ReturnRequestEntity? ReturnRequest { get; set; }
    }
}
