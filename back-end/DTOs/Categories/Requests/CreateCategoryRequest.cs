using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Categories.Requests
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(150, ErrorMessage = "Category name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
