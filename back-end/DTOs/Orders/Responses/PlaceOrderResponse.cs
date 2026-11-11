namespace back_end.DTOs.Orders.Responses
{
    public class PlaceOrderResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = "Queued";
    }
}
