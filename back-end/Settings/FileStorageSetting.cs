namespace back_end.Settings
{
    public class FileStorageSetting
    {
        public int MaxFileSizeMB { get; set; } = 5;
        public string[] AllowedContentTypes { get; set; } = [];
        public string ImagePrefix { get; set; } = "images";
    }
}