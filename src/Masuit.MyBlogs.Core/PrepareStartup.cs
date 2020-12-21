using Hangfire;
using Hangfire.Dashboard;
using JiebaNet.Segmenter;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Systems;
using Masuit.Tools.Win32;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using StackExchange.Profiling;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core
{
    public static class PrepareStartup
    {
        /// <summary>
        /// 初始化系统设置参数
        /// </summary>
        /// <param name="app"></param>
        internal static void InitSettings(this IApplicationBuilder app)
        {
            var dic = app.ApplicationServices.GetRequiredService<DataContext>().SystemSetting.ToDictionary(s => s.Name, s => s.Value);
            CommonHelper.SystemSettings.AddOrUpdate(dic);
        }

        internal static void UseLuceneSearch(this IApplicationBuilder app, IHostEnvironment env, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions)
        {
            Task.Run(() =>
            {
                Console.WriteLine("正在导入自定义词库...");
                double time = HiPerfTimer.Execute(() =>
                {
                    var posts = app.ApplicationServices.GetRequiredService<DataContext>().Post;
                    var set = posts.Select(p => p.Title).AsEnumerable().SelectMany(s => s.Split(',', '，', ' ', '+', '—', '(', ')', '：', '&', '（', '）', '-', '_', '[', ']')).Where(s => s.Length > 1).Union(posts.Select(p => $"{p.Label},{p.Keyword}").AsEnumerable().SelectMany(s => s.Split(','))).ToHashSet();
                    var lines = File.ReadAllLines(Path.Combine(env.ContentRootPath, "App_Data", "CustomKeywords.txt")).Union(set);
                    var segmenter = new JiebaSegmenter();
                    foreach (var word in lines)
                    {
                        segmenter.AddWord(word);
                    }
                });
                Console.WriteLine($"导入自定义词库完成，耗时{time}s");
                Windows.ClearMemorySilent();
            });

            string lucenePath = Path.Combine(env.ContentRootPath, luceneIndexerOptions.Path);
            if (!Directory.Exists(lucenePath) || Directory.GetFiles(lucenePath).Length < 1)
            {
                Console.WriteLine("索引库不存在，开始自动创建Lucene索引库...");
                hangfire.CreateLuceneIndex();
                Console.WriteLine("索引库创建完成！");
            }
        }

        public static void SetupHangfire(this IApplicationBuilder app)
        {
            app.UseHangfireServer().UseHangfireDashboard("/taskcenter", new DashboardOptions()
            {
                Authorization = new[]
                {
                    new MyRestrictiveAuthorizationFilter()
                }
            }); //配置hangfire
            HangfireJobInit.Start(); //初始化定时任务
        }

        public static void SetupHttpsRedirection(this IApplicationBuilder app, IConfiguration config)
        {
            if (bool.Parse(config["Https:Enabled"]))
            {
                app.UseHttpsRedirection();
            }

            switch (config["UseRewriter"])
            {
                case "NonWww":
                    app.UseRewriter(new RewriteOptions().AddRedirectToNonWww(301)); // URL重写
                    break;
                case "WWW":
                    app.UseRewriter(new RewriteOptions().AddRedirectToWww(301)); // URL重写
                    break;
            }
        }

        public static void SetupMiniProfile(this IServiceCollection services)
        {
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.EnableServerTimingHeader = true;
                options.ResultsAuthorize = req => req.HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo)?.IsAdmin == true;
                options.ResultsListAuthorize = options.ResultsAuthorize;
                options.IgnoredPaths.AddRange("/Assets/", "/Content/", "/fonts/", "/images/", "/ng-views/", "/Scripts/", "/static/", "/template/", "/cloud10.png", "/favicon.ico");
                options.PopupRenderPosition = RenderPosition.BottomLeft;
                options.PopupShowTimeWithChildren = true;
                options.PopupShowTrivial = true;
            }).AddEntityFramework();
        }

        public static void ConfigureOptions(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            }); //配置Cookie策略
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100MB
            }); //配置请求长度
            services.Configure<ForwardedHeadersOptions>(options => // X-Forwarded-For
            {
                options.ForwardLimit = null;
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            services.Configure<StaticFileOptions>(options =>
            {
                options.OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = "public,no-cache";
                    context.Context.Response.Headers[HeaderNames.Expires] = DateTime.Now.AddDays(7).ToString("R");
                };
                options.ContentTypeProvider = new FileExtensionContentTypeProvider(MimeMapper.MimeTypes);
                options.HttpsCompression = HttpsCompressionMode.Compress;
            }); // 配置静态资源文件类型和缓存
        }
    }

    /// <summary>
    /// hangfire授权拦截器
    /// </summary>
    public class MyRestrictiveAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// 授权校验
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize(DashboardContext context)
        {
#if DEBUG
            return true;
#endif
            var user = context.GetHttpContext().Session.Get<UserInfoDto>(SessionKey.UserInfo) ?? new UserInfoDto();
            return user.IsAdmin;
        }
    }
}