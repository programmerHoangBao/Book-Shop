using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Orders.Requests
{
    public class SearchOrdersRequest : GetStaffOrdersRequest
    {
        [Required]
        [MaxLength(255)]
        public string Key { get; set; } = string.Empty;
    }
}
