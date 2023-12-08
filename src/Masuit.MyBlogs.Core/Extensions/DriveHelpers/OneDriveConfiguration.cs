using Microsoft.Extensions.Primitives;

namespace Masuit.MyBlogs.Core.Extensions.DriveHelpers;

public sealed class OneDriveConfiguration
{
    private static IConfigurationRoot _configurationRoot;

    static OneDriveConfiguration()
    {
        BindConfig();
        ChangeToken.OnChange(_configurationRoot.GetReloadToken, BindConfig);
        return;

        void BindConfig()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, true);
            _configurationRoot = builder.Build();
        }
    }

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public static string ConnectionString => _configurationRoot["OneDrive:ConnectionString"];

    /// <summary>
    /// Graph连接 ClientID
    /// </summary>
    public static string ClientId => _configurationRoot["OneDrive:ClientId"];

    /// <summary>
    /// Graph连接 ClientSecret
    /// </summary>
    public static string ClientSecret => _configurationRoot["OneDrive:ClientSecret"];

    /// <summary>
    /// Binding 回调 Url
    /// </summary>
    public static string BaseUri => _configurationRoot["OneDrive:BaseUri"];

    /// <summary>
    /// 返回 Scopes
    /// </summary>
    /// <value></value>
    public static string[] Scopes => new[] { "Sites.ReadWrite.All", "Files.ReadWrite.All" };

    /// <summary>
    /// 代理路径
    /// </summary>
    public static string Proxy => _configurationRoot["OneDrive:Proxy"];

    /// <summary>
    /// 账户名称
    /// </summary>
    public static string AccountName => _configurationRoot["OneDrive:AccountName"];

    /// <summary>
    /// 域名
    /// </summary>
    public static string DominName => _configurationRoot["OneDrive:DominName"];

    /// <summary>
    /// Office 类型
    /// </summary>
    /// <param name="="></param>
    /// <returns></returns>
    public static OfficeType Type => (_configurationRoot["OneDrive:Type"] == "China") ? OfficeType.China : OfficeType.Global;

    /// <summary>
    /// Graph Api
    /// </summary>
    /// <param name="="></param>
    /// <returns></returns>
    public static string GraphApi => (_configurationRoot["OneDrive:Type"] == "China") ? "https://microsoftgraph.chinacloudapi.cn" : "https://graph.microsoft.com";

    /// <summary>
    /// CDN URL
    /// </summary>
    public static string[] CDNUrls => _configurationRoot.GetSection("OneDrive:CDNUrls").GetChildren().Select(x => x.Value).ToArray();

    public enum OfficeType
    {
        Global,
        China
    }
}
