using back_end.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace back_end.Entities
{
    public class ReviewEntity : IAuditable
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid BookId { get; set; }

        [Required]
        public Guid OrderId {  get; set; }

        [Required]
        public int Ratting { get; set; } = 0;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public UserEntity? User { get; set; }
        public BookEntity? Book { get; set; }
        public OrderEntity? Order { get; set; }

        public ICollection<ReviewImageEntity> Images { get; set; } = new List<ReviewImageEntity>();
    }
}
