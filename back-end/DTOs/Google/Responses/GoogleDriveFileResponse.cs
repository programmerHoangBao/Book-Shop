namespace back_end.DTOs.Google.Responses
{
    public class GoogleDriveFileResponse
    {
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string WebViewLink { get; set; } = string.Empty;
    }
}
