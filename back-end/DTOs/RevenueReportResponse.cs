namespace back_end.DTOs
{
    public class RevenueReportResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public List<RevenueReportItem> Items { get; set; } = new();
    }

    public class RevenueReportItem
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime? PeriodStart { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int QuantitySold { get; set; }
    }
}
