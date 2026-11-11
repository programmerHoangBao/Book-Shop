namespace back_end.DTOs.Books.Responses
{
    public class BookImportResult
    {
        public int RowNumber { get; set; }
        public string? BookIdentifier { get; set; }
        public Guid BookId { get; set; }
        public Guid InventoryId { get; set; }
        public int QuantityAdded { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
    }

    public class BookImportFailure
    {
        public int RowNumber { get; set; }
        public string? BookIdentifier { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class BooksImportResponse
    {
        public int TotalRows { get; set; }
        public List<BookImportResult> Imported { get; set; } = new();
        public List<BookImportFailure> Failed { get; set; } = new();
    }
}
