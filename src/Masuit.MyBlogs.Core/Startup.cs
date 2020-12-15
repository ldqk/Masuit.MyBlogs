using Autofac;
using Autofac.Extensions.DependencyInjection;
using CSRedis;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using JiebaNet.Segmenter;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Hubs;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Core.AspNetCore;
using Masuit.Tools.Core.Config;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Systems;
using Masuit.Tools.Win32;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using StackExchange.Profiling;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core
{
    /// <summary>
    /// asp.net core核心配置
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 依赖注入容器
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 配置中心
        /// </summary>
        public IConfiguration Configuration { get; set; }
        private readonly IWebHostEnvironment _env;
        /// <summary>
        /// asp.net core核心配置
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;

            void BindConfig()
            {
                Configuration = configuration;
                AppConfig.ConnString = configuration[nameof(AppConfig.ConnString)];
                AppConfig.BaiduAK = configuration[nameof(AppConfig.BaiduAK)];
                AppConfig.Redis = configuration[nameof(AppConfig.Redis)];
                AppConfig.TrueClientIPHeader = configuration[nameof(AppConfig.TrueClientIPHeader)] ?? "CF-Connecting-IP";
                AppConfig.EnableIPDirect = bool.Parse(configuration[nameof(AppConfig.EnableIPDirect)] ?? "false");
                configuration.Bind("Imgbed:AliyunOSS", AppConfig.AliOssConfig);
                configuration.Bind("Imgbed:Gitlabs", AppConfig.GitlabConfigs);
                configuration.AddToMasuitTools();
            }

            ChangeToken.OnChange(configuration.GetReloadToken, BindConfig);
            BindConfig();
        }

        /// <summary>
        /// ConfigureServices
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public void ConfigureServices(IServiceCollection services)
        {
            RedisHelper.Initialization(new CSRedisClient(AppConfig.Redis));
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            }); //配置Cookie策略
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseMySql(AppConfig.ConnString, ServerVersion.AutoDetect(AppConfig.ConnString), builder => builder.EnableRetryOnFailure(3)).EnableDetailedErrors();
                //opt.UseSqlServer(AppConfig.ConnString);
            }); //配置数据库
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100MB
            }); //配置请求长度
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });
            services.AddSession().AddAntiforgery(); //注入Session
            services.AddSignalR().AddNewtonsoftJsonProtocol();

            services.AddResponseCache().AddCacheConfig();
            services.AddHangfire((provider, configuration) =>
            {
                configuration.UseFilter(new AutomaticRetryAttribute());
                configuration.UseMemoryStorage();
            }); //配置hangfire

            services.AddSevenZipCompressor().AddResumeFileResult().AddSearchEngine<DataContext>(new LuceneIndexerOptions()
            {
                Path = "lucene"
            }); // 配置7z和断点续传和Redis和Lucene搜索引擎

            services.AddHttpClient("", c => c.Timeout = TimeSpan.FromSeconds(30)); //注入HttpClient
            services.AddHttpClient<ImagebedClient>();
            services.AddHttpContextAccessor(); //注入静态HttpContext
            services.AddMailSender(Configuration).AddFirewallReporter(Configuration);
            services.AddBundling().UseDefaults(_env).UseNUglify().EnableMinification().EnableChangeDetection().EnableCacheHeader(TimeSpan.FromHours(1));
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.EnableServerTimingHeader = true;
                options.ResultsAuthorize = req => req.HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo)?.IsAdmin ?? false;
                options.ResultsListAuthorize = options.ResultsAuthorize;
                options.IgnoredPaths.AddRange("/Assets/", "/Content/", "/fonts/", "/images/", "/ng-views/", "/Scripts/", "/static/", "/template/", "/cloud10.png", "/favicon.ico");
                options.PopupRenderPosition = RenderPosition.BottomLeft;
                options.PopupShowTimeWithChildren = true;
                options.PopupShowTrivial = true;
            }).AddEntityFramework();
            services.AddOneDrive();
            services.AddMapper().AddAutofac().AddMyMvc().Configure<ForwardedHeadersOptions>(options => // X-Forwarded-For
            {
                options.ForwardLimit = null;
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModule());
        }

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="db"></param>
        /// <param name="hangfire"></param>
        /// <param name="luceneIndexerOptions"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContext db, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions)
        {
            ServiceProvider = app.ApplicationServices;
            db.Database.EnsureCreated();
            InitSettings(db);
            UseLuceneSearch(env, hangfire, luceneIndexerOptions);

            app.UseForwardedHeaders().UseCertificateForwarding(); // X-Forwarded-For
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/ServiceUnavailable");
            }

            app.UseBundles();
            if (bool.Parse(Configuration["Https:Enabled"]))
            {
                app.UseHttpsRedirection();
            }

            switch (Configuration["UseRewriter"])
            {
                case "NonWww":
                    app.UseRewriter(new RewriteOptions().AddRedirectToNonWww(301)); // URL重写
                    break;
                case "WWW":
                    app.UseRewriter(new RewriteOptions().AddRedirectToWww(301)); // URL重写
                    break;
            }

            app.UseDefaultFiles().UseStaticFiles(new StaticFileOptions //静态资源缓存策略
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = "public,no-cache";
                    context.Context.Response.Headers[HeaderNames.Expires] = DateTime.Now.AddDays(7).ToString("R");
                },
                ContentTypeProvider = new FileExtensionContentTypeProvider(MimeMapper.MimeTypes),
            });
            app.UseSession().UseCookiePolicy().UseMiniProfiler(); //注入Session
            app.UseRequestIntercept(); //启用网站请求拦截

            app.UseHangfireServer().UseHangfireDashboard("/taskcenter", new DashboardOptions()
            {
                Authorization = new[]
                {
                    new MyRestrictiveAuthorizationFilter()
                }
            }); //配置hangfire
            app.UseResponseCaching().UseResponseCompression(); //启动Response缓存
            app.UseMiddleware<TranslateMiddleware>();
            //app.UseActivity();// 抽奖活动
            app.UseRouting(); // 放在 UseStaticFiles 之后
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // 属性路由
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"); // 默认路由
                endpoints.MapHub<MyHub>("/hubs");
            });

            HangfireJobInit.Start(); //初始化定时任务
            Console.WriteLine("网站启动完成");
        }

        private static void InitSettings(DataContext db)
        {
            var dic = db.SystemSetting.ToDictionary(s => s.Name, s => s.Value); //初始化系统设置参数
            foreach (var (key, value) in dic)
            {
                CommonHelper.SystemSettings.TryAdd(key, value);
            }
        }

        private static void UseLuceneSearch(IHostEnvironment env, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions)
        {
            Task.Run(() =>
            {
                Console.WriteLine("正在导入自定义词库...");
                double time = HiPerfTimer.Execute(() =>
                {
                    var set = ServiceProvider.GetRequiredService<DataContext>().Post.Select(p => $"{p.Title},{p.Label},{p.Keyword}").AsEnumerable().SelectMany(s => s.Split(new[] { ',', ' ', '+', '—' }, StringSplitOptions.RemoveEmptyEntries)).ToHashSet();
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