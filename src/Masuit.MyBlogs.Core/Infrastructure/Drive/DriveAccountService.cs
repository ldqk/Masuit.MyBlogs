using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Models.Drive;
using Microsoft.Identity.Client;

namespace Masuit.MyBlogs.Core.Infrastructure.Drive
{
    public class DriveAccountService : IDriveAccountService
    {
        private IConfidentialClientApplication app;
        public DriveContext SiteContext { get; set; }
        /// <summary>
        /// Graph实例
        /// </summary>
        /// <value></value>
        public Microsoft.Graph.GraphServiceClient Graph { get; set; }

        public DriveAccountService(DriveContext siteContext, TokenService tokenService)
        {
            this.SiteContext = siteContext;
            this.app = tokenService.app;
            this.Graph = tokenService.Graph;
        }
        /// <summary>
        /// 返回 Oauth 验证url
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAuthorizationRequestUrl()
        {
            var redirectUrl = await app.GetAuthorizationRequestUrl(OneDriveConfiguration.Scopes).ExecuteAsync();
            return redirectUrl.AbsoluteUri;
        }
        /// <summary>
        /// 添加 SharePoint Site-ID 到数据库
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="dominName"></param>
        /// <returns></returns>
        public async Task AddSiteId(string siteName, string nickName)
        {
            Site site = new Site();
            //使用 Onedrive
            if (siteName == "onedrive")
            {
                site.Name = siteName;
                site.NickName = nickName;
            }
            else
            {
                using HttpClient httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(20)
                };
                var apiCaller = new ProtectedApiCallHelper(httpClient);
                await apiCaller.CallWebApiAndProcessResultASync($"{OneDriveConfiguration.GraphApi}/v1.0/sites/{OneDriveConfiguration.DominName}:/sites/{siteName}", GetToken(), result =>
                {
                    site.SiteId = result.Properties().Single((prop) => prop.Name == "id").Value.ToString();
                    site.Name = result.Properties().Single((prop) => prop.Name == "name").Value.ToString();
                    site.NickName = nickName;
                });
            }
            if (!SiteContext.Sites.Any(s => s.SiteId == site.SiteId))
            {
                //若是首次添加则设置为默认的驱动器
                using (SettingService setting = new SettingService(new DriveContext()))
                {
                    if (!SiteContext.Sites.Any())
                    {
                        await setting.Set("DefaultDrive", site.Name);
                    }
                }
                await SiteContext.Sites.AddAsync(site);
                await SiteContext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("站点已被创建");
            }
        }

        public List<Site> GetSites()
        {
            List<Site> result = SiteContext.Sites.ToList();
            return result;
        }

        /// <summary>
        /// 获取 Drive Info
        /// </summary>
        /// <returns></returns>
        public async Task<List<DriveInfo>> GetDriveInfo()
        {
            List<DriveInfo> drivesInfo = new List<DriveInfo>();
            foreach (var item in SiteContext.Sites.ToArray())
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
            SiteContext.Sites.Remove(SiteContext.Sites.Single(site => site.NickName == nickName));
            await SiteContext.SaveChangesAsync();
        }

        /// <summary>
        /// 获取 Token
        /// </summary>
        /// <returns></returns>
        public string GetToken()
        {
            return app.AcquireTokenSilent(OneDriveConfiguration.Scopes, OneDriveConfiguration.AccountName).ExecuteAsync().Result.AccessToken;
        }
        public class DriveInfo
        {
            public Microsoft.Graph.Quota Quota { get; set; }
            public string NickName { get; set; }
            public string Name { get; set; }
            public string[] HiddenFolders { get; set; }
        }
    }
}