namespace back_end.Settings
{
    public class GoogleSetting
    {
        public string ClientId { get; set; } = string.Empty;
        public string CredentialsPath { get; set; } = string.Empty;
        public string FolderId {  get; set; } = string.Empty;
        public int MaxFileSizeMB {  get; set; }
        public bool ShareWithAnyone { get; set; }
        public string ApplicationName { get; set; } = string.Empty;
    }
}
