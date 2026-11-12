using back_end.Enums;
using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Promotions.Requests
{
    public class CreatePromotionRequest
    {
        [MaxLength(6)]
        public string? Code { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public PromotionType Type { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderValue { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int UsageLimit { get; set; }
        public bool FreeShipping { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Guid> BookIds { get; set; } = new();
        public List<Guid> CategoryIds { get; set; } = new();
    }
}
