using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Returns.Requests
{
    public class CreateReturnRequest
    {
        [Required]
        [MaxLength(255)]
        public string BankAccountName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string BankAccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<CreateReturnItemRequest> Items { get; set; } = new();

        [Required]
        [MinLength(1)]
        public List<string> EvidenceImageUrls { get; set; } = new();
    }

    public class CreateReturnItemRequest
    {
        public Guid OrderItemId { get; set; }
        public int Quantity { get; set; }
    }
}
