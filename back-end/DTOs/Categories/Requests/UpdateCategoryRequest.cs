using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Categories.Requests
{
    public class UpdateCategoryRequest
    {
        [Required]
        public Guid CategoryId { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
