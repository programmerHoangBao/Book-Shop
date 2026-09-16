using back_end.Entities.Interfaces;
using back_end.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class UserEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public RoleUser Role { get; set; } = RoleUser.Customer;

        [Column(TypeName = "varchar(11)")]
        public string? PhoneNumber { get; set; }

        [Required]
        public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;

        public string? AvatarUrl { get; set; }
        public bool Enabled { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<AddressEntity> Addresses { get; set; } = new List<AddressEntity>();
        public ICollection<InventoryTransactionEntity> InventoryTransactions { get; set; } = new List<InventoryTransactionEntity>();
        public StoreStaffEntity? StoreStaff { get; set; }
        public CartEntity? Cart { get; set; }

        public ICollection<PromotionUsageEntity> PromotionUsages { get; set;} = new List<PromotionUsageEntity>();
        public ICollection<OrderEntity> Orders { get; set; } = new List<OrderEntity>();
        public ICollection<ReviewEntity> Reviews { get; set; }= new List<ReviewEntity>();
        public ICollection<ReturnRequestEntity> ReturnRequests { get; set; } = new List<ReturnRequestEntity>();
        public ICollection<NotificationEntity> Notifications { get; set; } = new List<NotificationEntity>();
    }
}
