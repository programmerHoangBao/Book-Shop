namespace back_end.DTOs.Invoices.Responses
{
    public class InvoiceItemResponse
    {
        public Guid BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
