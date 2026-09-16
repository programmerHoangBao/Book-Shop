using System.ComponentModel.DataAnnotations;

namespace back_end.Entities
{
    public class ReturnRequestEvidenceEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();
        [Required]
        public Guid ReturnRequestId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public ReturnRequestEntity? ReturnRequest { get; set; }
    }
}
