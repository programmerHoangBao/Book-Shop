using back_end.DTOs;
using back_end.DTOs.Books.Requests;
using back_end.DTOs.Books.Responses;
using back_end.Exceptions;
using back_end.Records;
using back_end.Services;
using System.Globalization;
using System.Text.Json;

namespace back_end.Services.Implements
{
    public class BookImportService : IBookImportService
    {
        private readonly IBookService _bookService;

        public BookImportService(IBookService bookService)
        {
            _bookService = bookService;
        }

        public Task<ApiResponse<object?>> ImportNewBooksAsync(ImportBooksFileRequest request) =>
            ImportAsync(request, importNewBooks: true);

        public Task<ApiResponse<object?>> ImportExistingBooksAsync(ImportBooksFileRequest request) =>
            ImportAsync(request, importNewBooks: false);

        private async Task<ApiResponse<object?>> ImportAsync(
            ImportBooksFileRequest request,
            bool importNewBooks)
        {
            BooksImportResponse result = new();
            List<ParsedBookImportRow> rows;
            try
            {
                rows = await BookImportFileParser.ParseAsync(request.File);
            }
            catch (Exception exception) when (
                exception is JsonException or IOException or FormatException or System.Xml.XmlException)
            {
                result.TotalRows = 1;
                result.Failed.Add(new BookImportFailure
                {
                    RowNumber = 0,
                    BookIdentifier = request.File?.FileName,
                    ErrorCode = ErrorRecord.RequestInvalid.ResponseCode,
                    ErrorMessage = exception.Message
                });
                return ApiResponse<object?>.Response(MessageRecord.Success, data: result);
            }

            if (rows.Count == 0)
            {
                result.TotalRows = 1;
                result.Failed.Add(new BookImportFailure
                {
                    RowNumber = 0,
                    BookIdentifier = request.File?.FileName,
                    ErrorCode = ErrorRecord.RequestInvalid.ResponseCode,
                    ErrorMessage = "The file does not contain any book rows to import."
                });
                return ApiResponse<object?>.Response(MessageRecord.Success, data: result);
            }

            result.TotalRows = rows.Count;
            foreach (ParsedBookImportRow row in rows)
            {
                string? identifier = Get(row.Values, "bookid", "id", "name");
                if (row.ParseError != null)
                {
                    AddFailure(result, row.RowNumber, identifier,
                        ErrorRecord.RequestInvalid.ResponseCode, row.ParseError);
                    continue;
                }

                try
                {
                    ApiResponse<object?> response = importNewBooks
                        ? await _bookService.AddNewBookToInventoryAsync(CreateNewBookRequest(row.Values, request.StoreId))
                        : await AddExistingBookAsync(row.Values, request.StoreId);

                    if (!response.IsSuccess)
                    {
                        AddFailure(result, row.RowNumber, identifier,
                            response.ResponseCode, response.Message);
                        continue;
                    }

                    if (response.Data is not AddBookToInventoryResponse imported)
                    {
                        AddFailure(result, row.RowNumber, identifier,
                            ErrorRecord.InternalServerError.ResponseCode,
                            "The import completed without a book result.");
                        continue;
                    }

                    result.Imported.Add(new BookImportResult
                    {
                        RowNumber = row.RowNumber,
                        BookIdentifier = identifier,
                        BookId = imported.BookId,
                        InventoryId = imported.InventoryId,
                        QuantityAdded = imported.QuantityAdded,
                        TotalQuantity = imported.Quantity,
                        AvailableQuantity = imported.AvailableQuantity
                    });
                }
                catch (BusinessException exception)
                {
                    AddFailure(result, row.RowNumber, identifier,
                        exception.ErrorRecord.ResponseCode, exception.ErrorRecord.Message);
                }
                catch (Exception)
                {
                    AddFailure(result, row.RowNumber, identifier,
                        ErrorRecord.InternalServerError.ResponseCode,
                        "Import failed because of an unexpected server error.");
                }
            }

            return ApiResponse<object?>.Response(MessageRecord.Success, data: result);
        }

        private async Task<ApiResponse<object?>> AddExistingBookAsync(
            Dictionary<string, string?> values,
            Guid? storeId)
        {
            string? rawBookId = Get(values, "bookid", "id");
            if (!Guid.TryParse(rawBookId, out Guid bookId) || bookId == Guid.Empty)
            {
                throw InvalidRow("A valid bookId is required for an existing book.");
            }

            int quantity = ParseQuantity(Get(values, "quantity", "qty"));
            return await _bookService.AddExistingBookToInventoryAsync(bookId,
                new AddExistingBookToInventoryRequest { StoreId = storeId, Quantity = quantity });
        }

        private static AddBookToInventoryRequest CreateNewBookRequest(
            Dictionary<string, string?> values,
            Guid? storeId)
        {
            string name = Get(values, "name", "title")?.Trim() ?? string.Empty;
            string? rawCategoryId = Get(values, "categoryid", "category");
            if (string.IsNullOrWhiteSpace(name) ||
                !Guid.TryParse(rawCategoryId, out Guid categoryId) || categoryId == Guid.Empty)
            {
                throw InvalidRow("A book name and valid categoryId are required for a new book.");
            }
            if (name.Length > 500 ||
                (Get(values, "author")?.Length ?? 0) > 255 ||
                (Get(values, "publisher")?.Length ?? 0) > 255)
            {
                throw InvalidRow("Book name must be at most 500 characters; author and publisher must be at most 255 characters.");
            }

            string? rawPrice = Get(values, "price");
            decimal price = 0;
            if (!string.IsNullOrWhiteSpace(rawPrice) &&
                (!decimal.TryParse(rawPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out price) || price < 0))
            {
                throw InvalidRow("Price must be a non-negative number.");
            }

            return new AddBookToInventoryRequest
            {
                StoreId = storeId,
                Name = name,
                Author = Get(values, "author")?.Trim(),
                Publisher = Get(values, "publisher")?.Trim(),
                Description = Get(values, "description"),
                Price = price,
                CategoryId = categoryId,
                Quantity = ParseQuantity(Get(values, "quantity", "qty")),
                ImageUrls = (Get(values, "imageurls", "images") ?? string.Empty)
                    .Split(['|', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
            };
        }

        private static int ParseQuantity(string? rawQuantity)
        {
            if (!int.TryParse(rawQuantity, NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity) || quantity <= 0)
            {
                throw InvalidRow("Quantity must be a positive whole number.");
            }
            return quantity;
        }

        private static string? Get(
            Dictionary<string, string?> values,
            params string[] keys)
        {
            foreach (string key in keys)
            {
                if (values.TryGetValue(BookImportFileParser.NormalizeHeader(key), out string? value))
                {
                    return value;
                }
            }
            return null;
        }

        private static void AddFailure(
            BooksImportResponse result,
            int rowNumber,
            string? identifier,
            string errorCode,
            string errorMessage)
        {
            result.Failed.Add(new BookImportFailure
            {
                RowNumber = rowNumber,
                BookIdentifier = identifier,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            });
        }

        private static BusinessException InvalidRow(string message) => new(
            new ErrorRecord(
                ErrorRecord.RequestInvalid.ResponseCode,
                message,
                ErrorRecord.RequestInvalid.HttpStatus));
    }
}
