using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Models.Drive;
using Microsoft.Identity.Client;

namespace Masuit.MyBlogs.Core.Infrastructure.Drive;

public sealed class DriveAccountService(DriveContext siteContext, TokenService tokenService) : IDriveAccountService
{
    private readonly IConfidentialClientApplication _app = tokenService.App;

    /// <summary>
    /// Graph实例
    /// </summary>
    /// <value></value>
    public Microsoft.Graph.GraphServiceClient Graph { get; set; } = tokenService.Graph;

    public DriveContext SiteContext
    {
        get => siteContext;

        set
        {
        }
    }

    /// <summary>
    /// 返回 Oauth 验证url
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetAuthorizationRequestUrl()
    {
        var redirectUrl = await _app.GetAuthorizationRequestUrl(OneDriveConfiguration.Scopes).ExecuteAsync();
        return redirectUrl.AbsoluteUri;
    }

    /// <summary>
    /// 添加 SharePoint Site-ID 到数据库
    /// </summary>
    /// <param name="siteName"></param>
    /// <param name="nickName"></param>
    /// <returns></returns>
    public async Task AddSiteId(string siteName, string nickName)
    {
        Site site = new();

        //使用 Onedrive
        if (siteName == "onedrive")
        {
            site.Name = siteName;
            site.NickName = nickName;
        }
        else
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            var apiCaller = new ProtectedApiCallHelper(httpClient);
            await apiCaller.CallWebApiAndProcessResultASync($"{OneDriveConfiguration.GraphApi}/v1.0/sites/{OneDriveConfiguration.DominName}:/sites/{siteName}", GetToken(), result =>
            {
                site.SiteId = result.Properties().Single((prop) => prop.Name == "id").Value.ToString();
                site.Name = result.Properties().Single((prop) => prop.Name == "name").Value.ToString();
                site.NickName = nickName;
            });
        }
        if (!siteContext.Sites.Any(s => s.SiteId == site.SiteId))
        {
            //若是首次添加则设置为默认的驱动器
            using (var setting = new SettingService(new DriveContext()))
            {
                if (!siteContext.Sites.Any())
                {
                    await setting.Set("DefaultDrive", site.Name);
                }
            }
            await siteContext.Sites.AddAsync(site);
            await siteContext.SaveChangesAsync();
        }
        else
        {
            throw new Exception("站点已被创建");
        }
    }

    public List<Site> GetSites()
    {
        return siteContext.Sites.ToList();
    }

    /// <summary>
    /// 获取 Drive Info
    /// </summary>
    /// <returns></returns>
    public async Task<List<DriveInfo>> GetDriveInfo()
    {
        var drivesInfo = new List<DriveInfo>();
        foreach (var item in siteContext.Sites.ToArray())
        {
            Microsoft.Graph.Drive drive;

            //Onedrive
            if (string.IsNullOrEmpty(item.SiteId))
            {
                drive = await Graph.Me.Drive.Request().GetAsync();
            }
            else
            {
                drive = await Graph.Sites[item.SiteId].Drive.Request().GetAsync();
            }
            drivesInfo.Add(new DriveInfo()
            {
                Quota = drive.Quota,
                NickName = item.NickName,
                Name = item.Name,
                HiddenFolders = item.HiddenFolders
            });
        }
        return drivesInfo;
    }

    public async Task Unbind(string nickName)
    {
        siteContext.Sites.Remove(siteContext.Sites.Single(site => site.NickName == nickName));
        await siteContext.SaveChangesAsync();
    }

    /// <summary>
    /// 获取 Token
    /// </summary>
    /// <returns></returns>
    public string GetToken()
    {
        return _app.AcquireTokenSilent(OneDriveConfiguration.Scopes, OneDriveConfiguration.AccountName).ExecuteAsync().Result.AccessToken;
    }

    public class DriveInfo
    {
        public Microsoft.Graph.Quota Quota { get; set; }

        public string NickName { get; set; }

        public string Name { get; set; }

        public string[] HiddenFolders { get; set; }
    }
}
