namespace back_end.DTOs.Stores.Responses
{
    public class UpdateStoreResponse
    {
        public Guid Id { get; set; }
        public string StoreCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
