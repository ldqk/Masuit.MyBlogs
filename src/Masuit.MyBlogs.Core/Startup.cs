using Autofac;
using CLRStats;
using FreeRedis;
using Hangfire;
using Hangfire.MemoryStorage;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Config;
using Masuit.Tools.Core.AspNetCore;
using Masuit.Tools.Core.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using Newtonsoft.Json;
using Polly;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using System.Net;
using System.Text.RegularExpressions;

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
                AppConfig.ConnString = configuration["Database:" + nameof(AppConfig.ConnString)];
                AppConfig.BaiduAK = configuration[nameof(AppConfig.BaiduAK)];
                AppConfig.Redis = configuration[nameof(AppConfig.Redis)];
                AppConfig.TrueClientIPHeader = configuration[nameof(AppConfig.TrueClientIPHeader)] ?? "CF-Connecting-IP";
                AppConfig.EnableIPDirect = bool.Parse(configuration[nameof(AppConfig.EnableIPDirect)] ?? "false");
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
            services.AddDbContext<DataContext>((serviceProvider, opt) => opt.UseNpgsql(AppConfig.ConnString, builder => builder.EnableRetryOnFailure(10)).EnableSensitiveDataLogging()); //配置数据库
            services.AddDbContext<LoggerDbContext>(opt => opt.UseNpgsql(AppConfig.ConnString)); //配置数据库
            services.ConfigureOptions();
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });
            services.AddSession().AddAntiforgery(); //注入Session
            services.AddResponseCache().AddCacheConfig();
            services.AddHangfireServer().AddHangfire((serviceProvider, configuration) =>
            {
                configuration.UseActivator(new HangfireActivator(serviceProvider));
                configuration.UseFilter(new AutomaticRetryAttribute());
                configuration.UseMemoryStorage();
            }); //配置hangfire

            services.AddSevenZipCompressor().AddResumeFileResult().AddSearchEngine<DataContext>(new LuceneIndexerOptions()
            {
                Path = "lucene"
            }); // 配置7z和断点续传和Redis和Lucene搜索引擎

            services.AddHttpClient("").AddTransientHttpErrorPolicy(builder => builder.Or<TaskCanceledException>().Or<OperationCanceledException>().Or<TimeoutException>().OrResult(res => !res.IsSuccessStatusCode).RetryAsync(5)).ConfigurePrimaryHttpMessageHandler(() =>
            {
                if (bool.TryParse(Configuration["HttpClientProxy:Enabled"], out var b) && b)
                {
                    return new HttpClientHandler
                    {
                        Proxy = new WebProxy(Configuration["HttpClientProxy:Uri"], true)
                    };
                }

                return new HttpClientHandler();
            }); //注入HttpClient
            services.AddHttpClient<ImagebedClient>().AddTransientHttpErrorPolicy(builder => builder.Or<TaskCanceledException>().Or<OperationCanceledException>().Or<TimeoutException>().OrResult(res => !res.IsSuccessStatusCode).RetryAsync(3)); //注入HttpClient
            services.AddMailSender(Configuration).AddFirewallReporter(Configuration).AddRequestLogger(Configuration).AddPerfCounterManager(Configuration);
            services.AddBundling().UseDefaults(_env).UseNUglify().EnableMinification().EnableChangeDetection().EnableCacheHeader(TimeSpan.FromHours(1));
            services.AddSingleton<IRedisClient>(new RedisClient(AppConfig.Redis)
            {
                Serialize = JsonConvert.SerializeObject,
                Deserialize = JsonConvert.DeserializeObject
            });
            services.SetupMiniProfile();
            services.AddSingleton<IMimeMapper, MimeMapper>(p => new MimeMapper());
            services.AddOneDrive();
            services.AutoRegisterServices();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMapper().AddMyMvc().AddHealthChecks();
            services.AddImageSharp(options =>
                {
                    options.MemoryStreamManager = new RecyclableMemoryStreamManager();
                    options.BrowserMaxAge = TimeSpan.FromDays(7);
                    options.CacheMaxAge = TimeSpan.FromDays(365);
                    options.Configuration = SixLabors.ImageSharp.Configuration.Default;
                }).SetRequestParser<QueryCollectionRequestParser>()
                .Configure<PhysicalFileSystemCacheOptions>(options =>
                {
                    options.CacheRootPath = null;
                    options.CacheFolder = "static/image_cache";
                })
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheKey<UriRelativeLowerInvariantCacheKey>()
                .SetCacheHash<SHA256CacheHash>()
                .Configure<PhysicalFileSystemProviderOptions>(options =>
                {
                    options.ProviderRootPath = null;
                })
                .AddProvider<PhysicalFileSystemProvider>()
                .AddProcessor<ResizeWebProcessor>()
                .AddProcessor<FormatWebProcessor>()
                .AddProcessor<BackgroundColorWebProcessor>()
                .AddProcessor<QualityWebProcessor>()
                .AddProcessor<AutoOrientWebProcessor>();
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
        /// <param name="hangfire"></param>
        /// <param name="luceneIndexerOptions"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions, DataContext maindb, LoggerDbContext loggerdb)
        {
            ServiceProvider = app.ApplicationServices;
            maindb.Database.EnsureCreated();
            loggerdb.Database.EnsureCreated();
            app.InitSettings();
            app.UseLuceneSearch(env, hangfire, luceneIndexerOptions);
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
            app.SetupHttpsRedirection(Configuration);
            app.UseDefaultFiles().UseWhen(c => Regex.IsMatch(c.Request.Path.Value + "", @"(\.jpg|\.jpeg|\.png|\.bmp|\.webp|\.tiff|\.pbm)$", RegexOptions.IgnoreCase), builder => builder.UseImageSharp()).UseStaticFiles();
            app.UseSession().UseCookiePolicy(); //注入Session
            app.UseWhen(c => c.Session.Get<UserInfoDto>(SessionKey.UserInfo)?.IsAdmin == true, builder =>
            {
                builder.UseMiniProfiler();
                builder.UseCLRStatsDashboard();
            });
            app.UseWhen(c => !c.Request.Path.StartsWithSegments("/_blazor"), builder => builder.UseMiddleware<RequestInterceptMiddleware>()); //启用网站请求拦截
            app.SetupHangfire();
            app.UseResponseCaching().UseResponseCompression(); //启动Response缓存
            app.UseMiddleware<TranslateMiddleware>();
            app.UseRouting().UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub(options =>
                {
                    options.ApplicationMaxBufferSize = 4194304;
                    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(10);
                    options.TransportMaxBufferSize = 8388608;
                });
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers(); // 属性路由
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"); // 默认路由
                endpoints.MapFallbackToController("Index", "Error");
            });

            Console.WriteLine("网站启动完成");
        }
    }
}
