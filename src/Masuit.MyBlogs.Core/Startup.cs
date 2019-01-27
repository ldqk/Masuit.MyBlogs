using Autofac;
using Autofac.Extensions.DependencyInjection;
using CacheManager.Core;
using Common;
using EFSecondLevelCache.Core;
using Hangfire;
using Hangfire.Dashboard;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Controllers;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Hubs;
using Masuit.MyBlogs.Core.Infrastructure.Application;
using Masuit.MyBlogs.Core.Infrastructure.Repository;
using Masuit.MyBlogs.Core.Infrastructure.Services;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.AspNetCore;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Masuit.MyBlogs.Core
{
    /// <summary>
    /// asp.net core核心配置
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// asp.net core核心配置
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            AppConfig.ConnString = configuration[nameof(AppConfig.ConnString)];
            AppConfig.BaiduAK = configuration[nameof(AppConfig.BaiduAK)];
            AppConfig.Redis = configuration[nameof(AppConfig.Redis)];
            //AppConfig.EnableViewCompress = Convert.ToBoolean(configuration[nameof(AppConfig.EnableViewCompress)]);
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
                //opt.UseMySql(AppConfig.ConnString);
                opt.UseSqlServer(AppConfig.ConnString);
            }).AddTransient<IDbConnection>(p => new SqlConnection(AppConfig.ConnString)); //配置数据库
            services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(p =>
                {
                    p.AllowAnyHeader();
                    p.AllowAnyMethod();
                    p.AllowAnyOrigin();
                    p.AllowCredentials();
                });
            }); //配置跨域

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "API文档",
                    Version = "v1"
                });
                c.DescribeAllEnumsAsStrings();
                c.IncludeXmlComments(AppContext.BaseDirectory + "Masuit.MyBlogs.Core.xml");
            }); //配置swagger

            services.AddHttpClient(); //注入HttpClient
            services.AddHttpContextAccessor(); //注入静态HttpContext
            services.AddResponseCaching(); //注入响应缓存
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 2048000;
            }); //配置请求长度

            services.AddSession(); //注入Session
            services.AddHangfire(x => x.UseRedisStorage(AppConfig.Redis)); //配置hangfire

            services.AddSevenZipCompressor().AddResumeFileResult().AddDefaultRedisHelper(AppConfig.Redis); //配置7z和断点续传和Redis
            //配置EF二级缓存
            services.AddEFSecondLevelCache();
            // 配置EF二级缓存策略
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(typeof(ICacheManagerConfiguration), new CacheManager.Core.ConfigurationBuilder().WithJsonSerializer().WithMicrosoftMemoryCacheHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10)).Build());

            services.AddWebSockets(opt => opt.ReceiveBufferSize = 4096 * 1024).AddSignalR();

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });

            services.AddMvc().AddJsonOptions(opt =>
            {
                opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2).AddControllersAsServices();
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All)); //解决razor视图中中文被编码的问题

            ContainerBuilder builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterAssemblyTypes(typeof(BaseRepository<>).Assembly).AsImplementedInterfaces().Where(t => t.Name.EndsWith("Repository")).PropertiesAutowired().AsSelf().InstancePerDependency();
            builder.RegisterAssemblyTypes(typeof(BaseService<>).Assembly).AsImplementedInterfaces().Where(t => t.Name.EndsWith("Service")).PropertiesAutowired().AsSelf().InstancePerDependency();
            builder.RegisterAssemblyTypes(typeof(BaseController).Assembly).AsImplementedInterfaces().Where(t => t.Name.EndsWith("Controller")).PropertiesAutowired().AsSelf().InstancePerDependency(); //注册控制器为属性注入
            builder.RegisterType<BackgroundJobClient>().SingleInstance(); //指定生命周期为单例
            builder.RegisterType<HangfireBackJob>().As<IHangfireBackJob>().PropertiesAutowired(PropertyWiringOptions.PreserveSetValues).InstancePerDependency();
            AutofacContainer = new AutofacServiceProvider(builder.Build());
            return AutofacContainer;
        }

        /// <summary>
        /// 依赖注入容器
        /// </summary>
        public static IServiceProvider AutofacContainer { get; set; }

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="db"></param>
        /// <param name="appLifetime"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DataContext db, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseException();
            }
            app.UseFirewall(); //启用网站防火墙
            //db.Database.Migrate();
            CommonHelper.SystemSettings = db.SystemSetting.ToDictionary(s => s.Name, s => s.Value); //初始化系统设置参数

            app.UseStaticHttpContext(); //注入静态HttpContext对象
            app.UseSession(); //注入Session

            app.UseEFSecondLevelCache(); //启动EF二级缓存
            app.UseHttpsRedirection().UseStaticFiles().UseCookiePolicy();
            app.UseHangfireServer().UseHangfireDashboard("/taskcenter", new DashboardOptions()
            {
                Authorization = new[]
                {
                    new MyRestrictiveAuthorizationFilter()
                }
            }); //配置hangfire
            app.UseCors(builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
                builder.AllowCredentials();
            }); //配置跨域
            app.UseResponseCaching(); //启动Response缓存
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "懒得勤快的博客");
            }); //配置swagger
            app.UseSignalR(hub => hub.MapHub<MyHub>("/hubs"));
            HangfireJobInit.Start(); //初始化定时任务
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });
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
            UserInfoOutputDto user = context.GetHttpContext().Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            return user.IsAdmin;
        }
    }
}