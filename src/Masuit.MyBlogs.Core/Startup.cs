using Autofac;
using CLRStats;
using Dispose.Scope.AspNetCore;
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
using Masuit.Tools.Mime;
using Masuit.Tools.Config;
using Masuit.Tools.Core.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Web.DependencyInjection;
using System.Text.RegularExpressions;
using Masuit.Tools.AspNetCore.ModelBinder;
using EFCoreSecondLevelCacheInterceptor;

namespace Masuit.MyBlogs.Core;

/// <summary>
/// asp.net core核心配置
/// </summary>
public class Startup
{
    /// <summary>
    /// 配置中心
    /// </summary>
    public IConfiguration Configuration { get; set; }

    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// asp.net core核心配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="env"></param>
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _env = env;
        ChangeToken.OnChange(configuration.GetReloadToken, BindConfig);
        BindConfig();
        return;

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
    }

    /// <summary>
    /// ConfigureServices
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddEFSecondLevelCache(options => options.UseCustomCacheProvider<EFCoreCacheProvider>(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5)).SkipCacheInvalidationCommands(s => Regex.IsMatch(s, @"ViewCount|DisplayCount|LinkLoopback|LoginRecord|ClickRecord|VisitRecord|SearchDetails")).DisableLogging(true).UseCacheKeyPrefix("EFCache:"));
        services.AddDbContext<DataContext>((serviceProvider, opt) => opt.UseNpgsql(AppConfig.ConnString, builder => builder.EnableRetryOnFailure(10)).EnableSensitiveDataLogging().AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>())); //配置数据库
        services.AddDbContext<LoggerDbContext>(opt => opt.UseNpgsql(AppConfig.ConnString)); //配置数据库
        services.ConfigureOptions();
        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
        });
        services.AddSession().AddAntiforgery(); //注入Session
        services.AddResponseCache();
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

        services.SetupHttpClients(Configuration);
        services.AddMailSender(Configuration).AddFirewallReporter(Configuration).AddRequestLogger(Configuration).AddPerfCounterManager(Configuration);
        services.AddBundling().UseDefaults(_env).UseNUglify().EnableMinification().EnableChangeDetection().EnableCacheHeader(TimeSpan.FromHours(1));
        services.AddSingleton<IRedisClient>(new RedisClient(AppConfig.Redis)
        {
            Serialize = JsonConvert.SerializeObject,
            Deserialize = JsonConvert.DeserializeObject
        });
        services.SetupMiniProfile();
        services.AddSingleton<IMimeMapper, MimeMapper>(_ => new MimeMapper());
        services.AddOneDrive();
        services.AutoRegisterServices();
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddMapper().AddMyMvc().AddHealthChecks();
        services.SetupImageSharp();
        services.AddHttpContextAccessor();
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
    /// <param name="maindb"></param>
    /// <param name="loggerdb"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions, DataContext maindb, LoggerDbContext loggerdb)
    {
        maindb.Database.EnsureCreated();
        loggerdb.Database.EnsureCreated();
        app.InitSettings();
        app.UseDisposeScope();
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
        app.UseRouting().UseBodyOrDefaultModelBinder().UseEndpoints(endpoints =>
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
        var redisClient = app.ApplicationServices.GetRequiredService<IRedisClient>();
        var keys = redisClient.Keys("*Online*");
        if (keys.Length > 0)
        {
            redisClient.Del(keys);
        }
    }
}
