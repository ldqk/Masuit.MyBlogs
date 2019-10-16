namespace Masuit.MyBlogs.Core.Configs
{
    public class GitlabConfig
    {
        public bool Enabled { get; set; }

        public string ApiUrl { get; set; }
        public string RawUrl { get; set; }
        public string AccessToken { get; set; }
        public string Branch { get; set; }
        public int FileLimitSize { get; set; }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return AccessToken == ((GitlabConfig)obj).AccessToken;
        }

        /// <summary>
        /// 判等
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(GitlabConfig x, GitlabConfig y)
        {
            return x?.AccessToken == y?.AccessToken;
        }

        /// <summary>
        /// 判不等
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(GitlabConfig x, GitlabConfig y)
        {
            return x?.AccessToken != y?.AccessToken;
        }
    }
}