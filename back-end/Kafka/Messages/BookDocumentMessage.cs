using back_end.Enums;

namespace back_end.Kafka.Messages
{
    public class BookDocumentMessage
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Author { get; set; }

        public string? Publisher { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public BookStatus Status { get; set; }

        public long Rank { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
