using back_end.Enums;

namespace back_end.DTOs.Promotions.Responses
{
    public class PromotionResponse
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public PromotionType Type { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderValue { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public bool FreeShipping { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Guid> BookIds { get; set; } = new();
        public List<Guid> CategoryIds { get; set; } = new();
    }
}
