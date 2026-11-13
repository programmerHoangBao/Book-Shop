namespace back_end.DTOs.CustomerReports
{
    public class CustomerCountReportResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CustomerCount { get; set; }
    }
}
