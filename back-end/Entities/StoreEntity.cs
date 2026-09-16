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
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        [Column(TypeName = "varchar(11)")]
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<StoreStaffEntity> StoreStaffs { get; set; } = new List<StoreStaffEntity>();
        public ICollection<InventoryEntity> Inventories { get; set; } = new List<InventoryEntity>();
    }
}
