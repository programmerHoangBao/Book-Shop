namespace back_end.Settings
{
    public class S3Setting
    {
        public string Region { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string AccessMode { get; set; } = "StableUrl"; // "PresignedUrl"
        public int PresignedUrlExpirationMinutes { get; set; } = 15;
    }
}
