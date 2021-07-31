using Autofac;
using Autofac.Extensions.DependencyInjection;
using CLRStats;
using CSRedis;
using EFCoreSecondLevelCacheInterceptor;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Polly;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
            services.AddEFSecondLevelCache(options => options.UseCustomCacheProvider<MyEFCacheManagerCoreProvider>(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5)).DisableLogging(true));
            services.AddDbContextPool<DataContext>((serviceProvider, opt) =>
            {
                opt.UseMySql(AppConfig.ConnString, ServerVersion.AutoDetect(AppConfig.ConnString), builder => builder.EnableRetryOnFailure(3)).EnableDetailedErrors().UseLazyLoadingProxies().UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll).AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>());
            }); //配置数据库
            services.ConfigureOptions();
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });
            services.AddSession().AddAntiforgery(); //注入Session
            services.AddResponseCache().AddCacheConfig();
            services.AddHangfire((_, configuration) =>
            {
                configuration.UseFilter(new AutomaticRetryAttribute());
                configuration.UseMemoryStorage();
            }); //配置hangfire

            services.AddSevenZipCompressor().AddResumeFileResult().AddSearchEngine<DataContext>(new LuceneIndexerOptions()
            {
                Path = "lucene"
            }); // 配置7z和断点续传和Redis和Lucene搜索引擎

            services.AddHttpClient("", c => c.Timeout = TimeSpan.FromSeconds(30)).AddTransientHttpErrorPolicy(builder => builder.Or<TaskCanceledException>().Or<OperationCanceledException>().Or<TimeoutException>().OrResult(res => !res.IsSuccessStatusCode).RetryAsync(5)).ConfigurePrimaryHttpMessageHandler(() =>
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
            services.AddHttpClient<ImagebedClient>().AddTransientHttpErrorPolicy(builder => builder.Or<TaskCanceledException>().Or<OperationCanceledException>().Or<TimeoutException>().OrResult(res => !res.IsSuccessStatusCode).RetryAsync(5)); //注入HttpClient
            services.AddMailSender(Configuration).AddFirewallReporter(Configuration);
            services.AddBundling().UseDefaults(_env).UseNUglify().EnableMinification().EnableChangeDetection().EnableCacheHeader(TimeSpan.FromHours(1));
            services.SetupMiniProfile();
            services.AddSingleton<IMimeMapper, MimeMapper>(p => new MimeMapper());
            services.AddOneDrive();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMapper().AddAutofac().AddMyMvc();
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
            app.UseDefaultFiles().UseStaticFiles();
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
                endpoints.MapControllers(); // 属性路由
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"); // 默认路由
                endpoints.MapFallbackToController("Index", "Error");
            });

            Console.WriteLine("网站启动完成");
        }
    }
}