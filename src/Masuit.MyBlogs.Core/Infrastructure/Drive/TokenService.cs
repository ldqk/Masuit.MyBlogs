using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

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
            if (Configuration.Type == Configuration.OfficeType.China)
            {
                app = ConfidentialClientApplicationBuilder.Create(Configuration.ClientId).WithClientSecret(Configuration.ClientSecret).WithRedirectUri(Configuration.BaseUri + "/api/admin/bind/new").WithAuthority(AzureCloudInstance.AzureChina, "common").Build();
            }
            else
            {
                app = ConfidentialClientApplicationBuilder.Create(Configuration.ClientId).WithClientSecret(Configuration.ClientSecret).WithRedirectUri(Configuration.BaseUri + "/api/admin/bind/new").WithAuthority(AzureCloudInstance.AzurePublic, "common").Build();
            }

            //缓存Token
            TokenCacheHelper.EnableSerialization(app.UserTokenCache);
            //这里要传入一个 Scope 否则默认使用 https://graph.microsoft.com/.default
            //而导致无法使用世纪互联版本
            authProvider = new AuthorizationCodeProvider(app, Configuration.Scopes);
            //获取Token
            if (File.Exists(TokenCacheHelper.CacheFilePath))
            {
                authorizeResult = authProvider.ClientApplication.AcquireTokenSilent(Configuration.Scopes, Configuration.AccountName).ExecuteAsync().Result;
                //Debug.WriteLine(authorizeResult.AccessToken);
            }

            //启用代理
            if (!string.IsNullOrEmpty(Configuration.Proxy))
            {
                // Configure your proxy
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = new WebProxy(Configuration.Proxy),
                    UseDefaultCredentials = true
                };
                var httpProvider = new Microsoft.Graph.HttpProvider(httpClientHandler, false)
                {
                    OverallTimeout = TimeSpan.FromSeconds(10)
                };
                Graph = new Microsoft.Graph.GraphServiceClient($"{Configuration.GraphApi}/v1.0", authProvider, httpProvider);
            }
            else
            {
                Graph = new Microsoft.Graph.GraphServiceClient($"{Configuration.GraphApi}/v1.0", authProvider);
            }

            //定时更新Token
            _ = new Timer(o =>
              {
                  if (File.Exists(TokenCacheHelper.CacheFilePath))
                  {
                      //TODO:自动刷新 Token 失效
                      authorizeResult = authProvider.ClientApplication.AcquireTokenSilent(Configuration.Scopes, Configuration.AccountName).ExecuteAsync().Result;
                  }
              }, null, TimeSpan.FromSeconds(0), TimeSpan.FromHours(1));
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<AuthenticationResult> Authorize(string code)
        {
            AuthorizationCodeProvider authorizationCodeProvider = new AuthorizationCodeProvider(app);
            authorizeResult = await authorizationCodeProvider.ClientApplication.AcquireTokenByAuthorizationCode(Configuration.Scopes, code).ExecuteAsync();
            return authorizeResult;
        }
    }
}