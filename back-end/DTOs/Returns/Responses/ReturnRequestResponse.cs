using back_end.Enums;

namespace back_end.DTOs.Returns.Responses
{
    public class ReturnRequestResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public ReturnRequestStatus Status { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<ReturnRequestListItemResponse> Items { get; set; } = new();
        public List<string> EvidenceImageUrls { get; set; } = new();
    }
}
