using System.ComponentModel.DataAnnotations;

namespace back_end.Entities
{
    public class ReviewImageEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid ReviewId { get; set; } = Guid.CreateVersion7();

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public ReviewEntity? Review { get; set; }
    }
}
