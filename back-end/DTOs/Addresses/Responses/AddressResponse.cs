namespace back_end.DTOs.Addresses.Responses
{
    public class AddressResponse
    {
        public Guid Id { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressDetail { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
