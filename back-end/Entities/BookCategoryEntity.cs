using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using back_end.Entities.Interfaces;

namespace back_end.Entities
{
    public class BookCategoryEntity : IAuditable
    {
        public Guid BookId { get; set; }
        public Guid CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public BookEntity? Book { get; set; }
        public CategoryEntity? Category { get; set; }
    }
}
