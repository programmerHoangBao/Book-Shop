namespace back_end.DTOs.Stores.Responses
{
    public class StoreResponse
    {
        public Guid Id { get; set; }
        public string CodeStore { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
