using back_end.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class ReturnRequestEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [MaxLength(255)]
        public string BankAccountName {get; set;} = string.Empty;
        [Required]
        [MaxLength(30)]
        public string BankAccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string BankName {  get; set; } = string.Empty;

        [Required]
        public ReturnRequestStatus Status { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public UserEntity? User { get; set; }
        public OrderEntity? Order { get; set; }

        public ICollection<ReturnRequestEvidenceEntity> Evidences { get; set; } = new List<ReturnRequestEvidenceEntity>();
        public ICollection<ReturnItemEntity> Items {  get; set; } = new List<ReturnItemEntity>();
        public RefundEntity? RefundEntity { get; set; }
    }
}
