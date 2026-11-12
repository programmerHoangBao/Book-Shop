using back_end.Enums;

namespace back_end.DTOs.Returns.Responses
{
    public class ConfirmReturnResponse
    {
        public Guid ReturnRequestId { get; set; }
        public Guid OrderId { get; set; }
        public ReturnRequestStatus Status { get; set; }
        public DateTime ApprovedAt { get; set; }
        public bool WasAlreadyApproved { get; set; }
    }
}
