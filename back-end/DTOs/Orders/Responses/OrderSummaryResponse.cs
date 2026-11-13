namespace back_end.DTOs.Orders.Responses
{
    public class OrderSummaryResponse
    {
        public int NewOrders { get; set; }
        public int InProgressOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}
