using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Infrastructure.Drive
{
    public class TokenService
    {
        public AuthorizationCodeProvider authProvider;

        public AuthenticationResult authorizeResult;

        /// <summary>
        /// Graph实例
        /// </summary>
        /// <value></value>
        public Microsoft.Graph.GraphServiceClient Graph { get; set; }

        public IConfidentialClientApplication app;

        public TokenService()
        {
            if (OneDriveConfiguration.Type == OneDriveConfiguration.OfficeType.China)
            {
                app = ConfidentialClientApplicationBuilder.Create(OneDriveConfiguration.ClientId).WithClientSecret(OneDriveConfiguration.ClientSecret).WithRedirectUri(OneDriveConfiguration.BaseUri + "/api/admin/bind/new").WithAuthority(AzureCloudInstance.AzureChina, "common").Build();
            }
            else
            {
                app = ConfidentialClientApplicationBuilder.Create(OneDriveConfiguration.ClientId).WithClientSecret(OneDriveConfiguration.ClientSecret).WithRedirectUri(OneDriveConfiguration.BaseUri + "/api/admin/bind/new").WithAuthority(AzureCloudInstance.AzurePublic, "common").Build();
            }

            //缓存Token
            TokenCacheHelper.EnableSerialization(app.UserTokenCache);
            //这里要传入一个 Scope 否则默认使用 https://graph.microsoft.com/.default
            //而导致无法使用世纪互联版本
            authProvider = new AuthorizationCodeProvider(app, OneDriveConfiguration.Scopes);
            //获取Token
            if (File.Exists(TokenCacheHelper.CacheFilePath))
            {
                authorizeResult = authProvider.ClientApplication.AcquireTokenSilent(OneDriveConfiguration.Scopes, OneDriveConfiguration.AccountName).ExecuteAsync().Result;
                //Debug.WriteLine(authorizeResult.AccessToken);
            }

            //启用代理
            if (!string.IsNullOrEmpty(OneDriveConfiguration.Proxy))
            {
                // Configure your proxy
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = new WebProxy(OneDriveConfiguration.Proxy),
                    UseDefaultCredentials = true
                };
                var httpProvider = new Microsoft.Graph.HttpProvider(httpClientHandler, false)
                {
                    OverallTimeout = TimeSpan.FromSeconds(10)
                };
                Graph = new Microsoft.Graph.GraphServiceClient($"{OneDriveConfiguration.GraphApi}/v1.0", authProvider, httpProvider);
            }
            else
            {
                Graph = new Microsoft.Graph.GraphServiceClient($"{OneDriveConfiguration.GraphApi}/v1.0", authProvider);
            }

            //定时更新Token
            _ = new Timer(_ =>
              {
                  if (File.Exists(TokenCacheHelper.CacheFilePath))
                  {
                      authorizeResult = authProvider.ClientApplication.AcquireTokenSilent(OneDriveConfiguration.Scopes, OneDriveConfiguration.AccountName).ExecuteAsync().Result;
                  }
              }, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<AuthenticationResult> Authorize(string code)
        {
            var authorizationCodeProvider = new AuthorizationCodeProvider(app);
            authorizeResult = await authorizationCodeProvider.ClientApplication.AcquireTokenByAuthorizationCode(OneDriveConfiguration.Scopes, code).ExecuteAsync();
            return authorizeResult;
        }
    }
}