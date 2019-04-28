namespace Masuit.MyBlogs.Core.Configs
{
    public class AliOssConfig
    {
        public bool Enabled { get; set; }
        public string EndPoint { get; set; }
        public string BucketDomain { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string BucketName { get; set; }
    }
}