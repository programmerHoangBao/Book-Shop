using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class StoreEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        [MaxLength(10)]
        public string StoreCode = string.Empty; //Example: BS01, BS02, BS03,...

        [Required]
        [MaxLength(1000)]
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<StoreStaffEntity> StoreStaffs { get; set; } = new List<StoreStaffEntity>();
        public ICollection<InventoryEntity> Inventories { get; set; } = new List<InventoryEntity>();
    }
}
