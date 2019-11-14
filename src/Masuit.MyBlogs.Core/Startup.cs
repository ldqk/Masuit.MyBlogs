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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
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

        /// <summary>
        /// asp.net core核心配置
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            AppConfig.ConnString = configuration[nameof(AppConfig.ConnString)];
            AppConfig.BaiduAK = configuration[nameof(AppConfig.BaiduAK)];
            AppConfig.Redis = configuration[nameof(AppConfig.Redis)];
            configuration.Bind("Imgbed:AliyunOSS", AppConfig.AliOssConfig);
            configuration.Bind("Imgbed:Gitlabs", AppConfig.GitlabConfigs);
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
                options.MinimumSameSitePolicy = SameSiteMode.None;
            }); //配置Cookie策略
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseMySql(AppConfig.ConnString);
                //opt.UseSqlServer(AppConfig.ConnString);
            }); //配置数据库
            services.AddCors(opt => opt.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())); //配置跨域
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100MB
            }); //配置请求长度
            services.AddSession(); //注入Session
            services.AddWebSockets(opt => opt.ReceiveBufferSize = 4096 * 1024).AddSignalR().AddNewtonsoftJsonProtocol();
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });

            services.AddResponseCache().AddCacheConfig();
            services.AddHangfire(x => x.UseMemoryStorage()); //配置hangfire

            services.AddSevenZipCompressor().AddResumeFileResult().AddSearchEngine<DataContext>(new LuceneIndexerOptions()
            {
                Path = "lucene"
            }); // 配置7z和断点续传和Redis和Lucene搜索引擎

            services.AddHttpClient("", c => c.Timeout = TimeSpan.FromSeconds(30)); //注入HttpClient
            services.AddTransient<ImagebedClient>();
            services.AddHttpContextAccessor(); //注入静态HttpContext
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //db.Database.Migrate();
            var dic = db.SystemSetting.ToDictionary(s => s.Name, s => s.Value); //初始化系统设置参数
            foreach (var (key, value) in dic)
            {
                CommonHelper.SystemSettings.TryAdd(key, value);
            }

            UseLuceneSearch(env, hangfire, luceneIndexerOptions);
            if (bool.Parse(Configuration["Https:Enabled"]))
            {
                app.UseHttpsRedirection().UseRewriter(new RewriteOptions().AddRedirectToNonWww()); // URL重写
            }

            app.UseSession().UseCookiePolicy(); //注入Session
            app.UseRequestIntercept(); //启用网站请求拦截
            app.UseStaticHttpContext(); //注入静态HttpContext对象
            app.UseStaticFiles(new StaticFileOptions //静态资源缓存策略
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = "public,no-cache";
                    context.Context.Response.Headers[HeaderNames.Expires] = DateTime.UtcNow.AddDays(7).ToString("R");
                },
                ContentTypeProvider = new FileExtensionContentTypeProvider(MimeMapper.MimeTypes),
            });

            app.UseHangfireServer().UseHangfireDashboard("/taskcenter", new DashboardOptions()
            {
                Authorization = new[]
                {
                    new MyRestrictiveAuthorizationFilter()
                }
            }); //配置hangfire
            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()); //配置跨域
            app.UseResponseCaching().UseResponseCompression(); //启动Response缓存
            app.UseRouting(); // 放在 UseStaticFiles 之后
            app.UseEndpoints(endpoints =>
           {
               endpoints.MapControllers(); // 属性路由
               endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"); // 默认路由
               endpoints.MapHub<MyHub>("/hubs");
           });
            HangfireJobInit.Start(); //初始化定时任务
        }

        private static void UseLuceneSearch(IWebHostEnvironment env, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions)
        {
            Task.Run(() =>
            {
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
            var user = context.GetHttpContext().Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            return user.IsAdmin;
        }
    }
}