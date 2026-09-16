using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using back_end.Enums;

namespace back_end.Entities
{
    public class InventoryTransactionEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        public Guid InventoryId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }
        [Required]
        public int Quantity { get; set; } = 0;
        [Required]
        public int QuntityBefore { get; set; } = 0;
        [Required]
        public int QuantityAfter { get; set; } = 0;
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public InventoryEntity? Inventory { get; set; }
        public UserEntity? User { get; set; }
    }
}
