namespace back_end.DTOs.Returns.Responses
{
    public class ReturnRequestListItemResponse
    {
        public Guid OrderItemId { get; set; }
        public Guid BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
