namespace Masuit.MyBlogs.Core.Configs
{
    public sealed class GitlabConfig
    {
        public bool Enabled { get; set; }

        public string ApiUrl { get; set; }
        public string RawUrl { get; set; }
        public string AccessToken { get; set; }
        public string Branch { get; set; }
        public int FileLimitSize { get; set; }
    }
}