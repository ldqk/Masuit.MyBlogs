using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Configs
{
    /// <summary>
    /// 应用程序配置
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnString { get; set; }

        /// <summary>
        /// 百度AK
        /// </summary>
        public static string BaiduAK { get; set; }

        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public static string Redis { get; set; }

        /// <summary>
        /// OSS配置
        /// </summary>
        public static AliOssConfig AliOssConfig { get; set; } = new AliOssConfig();

        /// <summary>
        /// gitlab图床配置
        /// </summary>
        public static GitlabConfig GitlabConfig { get; set; } = new GitlabConfig();

        /// <summary>
        /// 图床域名
        /// </summary>
        public static List<string> ImgbedDomains { get; set; } = new List<string>();
    }
}