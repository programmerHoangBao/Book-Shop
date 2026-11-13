namespace back_end.DTOs.CustomerReports
{
    public class CustomerPurchaseFrequencyResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalCompletedOrders { get; set; }
        public int PurchasingCustomerCount { get; set; }
        public decimal AveragePurchasesPerCustomer { get; set; }
        public List<CustomerPurchaseFrequencyItem> FrequencyDistribution { get; set; } = new();
    }

    public class CustomerPurchaseFrequencyItem
    {
        public int CompletedOrderCount { get; set; }
        public int CustomerCount { get; set; }
    }
}
