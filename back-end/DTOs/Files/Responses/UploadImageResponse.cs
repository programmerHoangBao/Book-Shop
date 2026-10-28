namespace back_end.DTOs.Files.Responses
{
    public class UploadImageResponse
    {
        public string ObjectKey { get; set; }
        public string ImageUrl { get; set; }
        public string ContentType { get; set; }
        public long SizeBytes { get; set; }

        public UploadImageResponse(
            string objectKey, 
            string imageUrl, 
            string contentType, 
            long sizeBytes
        )
        {
            ObjectKey = objectKey;
            ImageUrl = imageUrl;
            ContentType = contentType;
            SizeBytes = sizeBytes;
        }
    }
}
