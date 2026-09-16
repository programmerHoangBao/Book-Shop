using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class AddressEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(11)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string AddressDetail { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Ward { get; set; }
        [MaxLength(50)]
        public string? District { get; set; }
        [MaxLength(50)]
        public string? Province { get; set; }
        [MaxLength(50)]
        public string? City { get; set; }

        [Required]
        public bool IsDefault { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public UserEntity? User { get; set; }
        public ICollection<OrderEntity> Orders { get; set; } = new List<OrderEntity>();
    }
}
