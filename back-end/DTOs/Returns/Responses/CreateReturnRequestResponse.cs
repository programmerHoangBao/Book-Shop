using back_end.Enums;

namespace back_end.DTOs.Returns.Responses
{
    public class CreateReturnRequestResponse
    {
        public Guid ReturnRequestId { get; set; }
        public Guid OrderId { get; set; }
        public ReturnRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
