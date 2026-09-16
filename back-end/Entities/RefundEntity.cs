using back_end.Enums;
using System.ComponentModel.DataAnnotations;

namespace back_end.Entities
{
    public class RefundEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid ReturnRequestId { get; set; }
        public RefundStatus Status { get; set; }
        public decimal RefundAmount { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public ReturnRequestEntity? ReturnRequest { get; set; }
    }
}
