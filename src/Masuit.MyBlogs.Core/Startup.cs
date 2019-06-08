using Autofac;
using Autofac.Extensions.DependencyInjection;
using CacheManager.Core;
using CSRedis;
using EFSecondLevelCache.Core;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using JiebaNet.Segmenter;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Hubs;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Core.AspNetCore;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
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
        public static IServiceProvider AutofacContainer { get; set; }

        /// <summary>
        /// asp.net core核心配置
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            AppConfig.ConnString = configuration[nameof(AppConfig.ConnString)];
            AppConfig.BaiduAK = configuration[nameof(AppConfig.BaiduAK)];
            AppConfig.Redis = configuration[nameof(AppConfig.Redis)];
            configuration.Bind("Imgbed:AliyunOSS", AppConfig.AliOssConfig);
            configuration.Bind("Imgbed:Gitlab", AppConfig.GitlabConfig);
            configuration.Bind("Imgbed:Gitee", AppConfig.GiteeConfig);
            configuration.Bind("Imgbed:ImgbedDomains", AppConfig.ImgbedDomains);
        }

        /// <summary>
        /// ConfigureServices
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
            }); //配置Cookie策略
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseMySql(AppConfig.ConnString);
                //opt.UseSqlServer(AppConfig.ConnString);
            }); //配置数据库
            services.AddCors(opt => opt.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials())); //配置跨域

            services.AddHttpClient(); //注入HttpClient
            services.AddHttpContextAccessor(); //注入静态HttpContext
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100MB
            }); //配置请求长度

            ConfigureResponse(services);

            services.AddSession(); //注入Session
            services.AddHangfire(x => x.UseMemoryStorage()); //配置hangfire

            services.AddSevenZipCompressor().AddResumeFileResult().AddSearchEngine<DataContext>(new LuceneIndexerOptions()
            {
                Path = "lucene"
            }); // 配置7z和断点续传和Redis和Lucene搜索引擎
            RedisHelper.Initialization(new CSRedisClient(AppConfig.Redis));

            //配置EF二级缓存
            services.AddEFSecondLevelCache();
            // 配置EF二级缓存策略
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(new CacheManager.Core.ConfigurationBuilder().WithJsonSerializer().WithMicrosoftMemoryCacheHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10)).Build());

            services.AddWebSockets(opt => opt.ReceiveBufferSize = 4096 * 1024).AddSignalR();

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });

            services.AddMvc().AddJsonOptions(opt =>
            {
                opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2).AddControllersAsServices().AddViewComponentsAsServices().AddTagHelpersAsServices();

            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            }); //解决razor视图中中文被编码的问题

            ContainerBuilder builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsImplementedInterfaces().Where(t => t.Name.EndsWith("Repository") || t.Name.EndsWith("Service") || t.Name.EndsWith("Controller")).PropertiesAutowired().AsSelf().InstancePerDependency(); //注册控制器为属性注入
            builder.RegisterType<BackgroundJobClient>().SingleInstance(); //指定生命周期为单例
            builder.RegisterType<HangfireBackJob>().As<IHangfireBackJob>().PropertiesAutowired(PropertyWiringOptions.PreserveSetValues).InstancePerDependency();
            AutofacContainer = new AutofacServiceProvider(builder.Build());
            return AutofacContainer;
        }

        private static void ConfigureResponse(IServiceCollection services)
        {
            services.AddResponseCaching(); //注入响应缓存
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<CustomCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "image/svg+xml"
                });
            });
        }

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="db"></param>
        /// <param name="hangfire"></param>
        /// <param name="luceneIndexerOptions"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DataContext db, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseException();
            }

            //db.Database.Migrate();

            ConfigureLuceneSearch(env, hangfire, luceneIndexerOptions);

            app.UseResponseCompression();
            app.UseHttpsRedirection().UseRewriter(new RewriteOptions().AddRedirectToNonWww()); // URL重写
            app.UseStaticHttpContext(); //注入静态HttpContext对象
            app.UseSession().UseCookiePolicy(); //注入Session

            app.UseStaticFiles(new StaticFileOptions //静态资源缓存策略
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = "public,no-cache";
                    context.Context.Response.Headers[HeaderNames.Expires] = DateTime.UtcNow.AddDays(7).ToString("R");
                },
                ContentTypeProvider = new FileExtensionContentTypeProvider(MimeMapper.MimeTypes),
            });

            app.UseFirewall().UseRequestIntercept(); //启用网站防火墙
            var dic = db.SystemSetting.ToDictionary(s => s.Name, s => s.Value); //初始化系统设置参数
            foreach (var (key, value) in dic)
            {
                CommonHelper.SystemSettings.TryAdd(key, value);
            }
            app.UseHangfireServer().UseHangfireDashboard("/taskcenter", new DashboardOptions()
            {
                Authorization = new[] { new MyRestrictiveAuthorizationFilter() }
            }); //配置hangfire
            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials()); //配置跨域
            app.UseResponseCaching(); //启动Response缓存
            app.UseSignalR(hub => hub.MapHub<MyHub>("/hubs"));
            HangfireJobInit.Start(); //初始化定时任务
            app.UseMvcWithDefaultRoute();
        }

        private static void ConfigureLuceneSearch(IHostingEnvironment env, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions)
        {
            #region 导词库

            Console.WriteLine("正在导入自定义词库...");
            double time = HiPerfTimer.Execute(() =>
            {
                var lines = File.ReadAllLines(Path.Combine(env.ContentRootPath, "App_Data", "CustomKeywords.txt"));
                var segmenter = new JiebaSegmenter();
                foreach (var word in lines)
                {
                    segmenter.AddWord(word);
                }
            });
            Console.WriteLine($"导入自定义词库完成，耗时{time}s");

            #endregion

            string lucenePath = Path.Combine(env.ContentRootPath, luceneIndexerOptions.Path);
            if (!Directory.Exists(lucenePath) || Directory.GetFiles(lucenePath).Length < 1)
            {
                Console.WriteLine("，索引库不存在，开始自动创建Lucene索引库...");
                hangfire.CreateLuceneIndex();
                Console.WriteLine("索引库创建完成！");
            }
        }
    }

    public class CustomCompressionProvider : ICompressionProvider
    {
        public string EncodingName => "mycustomcompression";
        public bool SupportsFlush => true;

        public Stream CreateStream(Stream outputStream)
        {
            // Create a custom compression stream wrapper here
            return outputStream;
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
            UserInfoOutputDto user = context.GetHttpContext().Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            return user.IsAdmin;
        }
    }
}