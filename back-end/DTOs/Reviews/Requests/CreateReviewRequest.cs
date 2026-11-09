using System.ComponentModel.DataAnnotations;

namespace back_end.DTOs.Reviews.Requests
{
    public class CreateReviewRequest
    {
        [Required(ErrorMessage = "BookId is required!")]
        public Guid BookId { get; set; }
        [Required(ErrorMessage = "OrderId is required!")]
        public Guid OrderId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public List<string>? ImageUrls { get; set; }
    }
}
