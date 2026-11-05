namespace back_end.Settings
{
    public class ElasticsearchSetting
    {
        public string Url { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DefaultIndex {  get; set; } = string.Empty;
    }
}
