namespace back_end.DTOs.Books.Requests
{
    public class ImportBooksFileRequest
    {
        public IFormFile? File { get; set; }

        public Guid? StoreId { get; set; }
    }
}
