using System.ComponentModel.DataAnnotations;

namespace back_end.Entities
{
    public class ReturnItemEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();
        [Required]
        public Guid ReturnRequestId { get; set; }
        [Required]
        public Guid OrderItemId { get; set; }
        public int Quantity { get; set; }

        public ReturnRequestEntity? ReturnRequest { get; set; }
        public OrderItemEntity? OrderItem { get; set; }
    }
}
