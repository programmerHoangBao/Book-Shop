namespace back_end.DTOs.Merchandise
{
    public class MerchandiseBookReportItem
    {
        public Guid BookId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Author { get; set; }
        public int? AvailableQuantity { get; set; }
        public int QuantitySold { get; set; }
    }
}
