using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Entities
{
    public class BookImageEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();
        [Required]
        public Guid BookId { get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public BookEntity? Book { get; set; }
    }
}
