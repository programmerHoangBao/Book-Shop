using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class InventoryEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid StoreId { get; set; }

        [Required]
        public Guid BookId { get; set; }
        [Required]
        public int Quantity { get; set; } = 0;
        [Required]
        public int ReservedQuantity { get; set; } = 0;
        [Required]
        public int AvailableQuantity { get; set; } = 0;
        [Required]
        public int DamagedQuantity { get; set; } = 0;
        [Required]
        public int IncomingQuantity { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public StoreEntity? Store { get; set; }
        public BookEntity? Book { get; set; }
        public ICollection<InventoryTransactionEntity> transactions { get; set; } = new List<InventoryTransactionEntity>();
    }
}
