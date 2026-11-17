using back_end.Enums;
using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Orders.Requests
{
    public class GetStaffOrdersRequest
    {
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus? Status { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}
