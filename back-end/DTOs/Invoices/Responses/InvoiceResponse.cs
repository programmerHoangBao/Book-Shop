using back_end.Enums;

namespace back_end.DTOs.Invoices.Responses
{
    public class InvoiceResponse
    {
        public Guid InvoiceId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime OrderedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<InvoiceItemResponse> Items { get; set; } = new();
    }
}
