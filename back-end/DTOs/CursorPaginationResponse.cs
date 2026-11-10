namespace back_end.DTOs
{
    public class CursorPaginationResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public string? NextCursor { get; set; }
    }
}
