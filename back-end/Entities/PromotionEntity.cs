using back_end.Entities.Interfaces;
using back_end.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class PromotionEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Column(TypeName = "varchar(6)")]
        public string? Code { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public PromotionType Type { get; set; }

        [Required]
        public decimal DiscountValue { get; set; }
        [Required]
        public decimal MinimumOrderValue { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public int UsageLimit { get; set; }
        public int UsageCount { get; set; }

        public bool FreeShipping { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<PromotionBookEntity> PromotionBooks { get; set; } = new List<PromotionBookEntity>();
        public ICollection<PromotionCatatogyEntity> PromotionCatatogies { get; set; } = new List<PromotionCatatogyEntity>();
        public ICollection<PromotionUsageEntity> PromotionUsages { get; set;} = new List<PromotionUsageEntity>();
    }
}
