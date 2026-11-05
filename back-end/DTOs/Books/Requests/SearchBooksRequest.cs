using back_end.Enums;

namespace back_end.DTOs.Books.Requests
{
    public class SearchBooksRequest
    {
        public string? Key { get; set; }

        public BookStatus? Book { get; set; }

        public bool? IsDeleted { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
