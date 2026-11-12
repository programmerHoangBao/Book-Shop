using back_end.Enums;
using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Orders.Requests
{
    public class PlaceOrderRequest
    {
        [Required]
        public Guid AddressId { get; set; }

        [EnumDataType(typeof(PaymentMethod))]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        [MaxLength(6)]
        public string? PromotionCode { get; set; }
    }
}
