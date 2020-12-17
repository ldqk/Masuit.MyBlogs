using Microsoft.Extensions.Configuration;
using System.IO;

namespace Masuit.MyBlogs.Core.Extensions.DriveHelpers
{
    public class OneDriveConfiguration
    {
        private static readonly IConfigurationRoot ConfigurationRoot;
        static OneDriveConfiguration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, true);
            ConfigurationRoot = builder.Build();
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString => ConfigurationRoot["OneDrive:ConnectionString"];

        /// <summary>
        /// Graph连接 ClientID
        /// </summary>
        public static string ClientId => ConfigurationRoot["OneDrive:ClientId"];

        /// <summary>
        /// Graph连接 ClientSecret
        /// </summary>
        public static string ClientSecret => ConfigurationRoot["OneDrive:ClientSecret"];

        /// <summary>
        /// Binding 回调 Url
        /// </summary>
        public static string BaseUri => ConfigurationRoot["OneDrive:BaseUri"];

        /// <summary>
        /// 返回 Scopes
        /// </summary>
        /// <value></value>
        public static string[] Scopes => new[] { "Sites.ReadWrite.All", "Files.ReadWrite.All" };

        /// <summary>
        /// 代理路径
        /// </summary>
        public static string Proxy => ConfigurationRoot["OneDrive:Proxy"];

        /// <summary>
        /// 账户名称
        /// </summary>
        public static string AccountName => ConfigurationRoot["OneDrive:AccountName"];

        /// <summary>
        /// 域名
        /// </summary>
        public static string DominName => ConfigurationRoot["OneDrive:DominName"];

        /// <summary>
        /// Office 类型
        /// </summary>
        /// <param name="="></param>
        /// <returns></returns>
        public static OfficeType Type => (ConfigurationRoot["OneDrive:Type"] == "China") ? OfficeType.China : OfficeType.Global;

        /// <summary>
        /// Graph Api
        /// </summary>
        /// <param name="="></param>
        /// <returns></returns>
        public static string GraphApi => (ConfigurationRoot["OneDrive:Type"] == "China") ? "https://microsoftgraph.chinacloudapi.cn" : "https://graph.microsoft.com";

        public enum OfficeType
        {
            Global,
            China
        }
    }
}