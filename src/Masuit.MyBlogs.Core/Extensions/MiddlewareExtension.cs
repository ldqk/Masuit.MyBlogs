using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using CacheManager.Core;
using Masuit.MyBlogs.Core.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Masuit.MyBlogs.Core.Extensions
{
    public static class MiddlewareExtension
    {
        /// <summary>
        /// 缓存配置
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCacheConfig(this IServiceCollection services)
        {
            var jss = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(new ConfigurationBuilder().WithRedisConfiguration("redis", AppConfig.Redis).WithJsonSerializer(jss, jss).WithMaxRetries(5).WithRetryTimeout(100).WithRedisCacheHandle("redis").WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(5))
                .Build());
            return services;
        }

        /// <summary>
        /// automapper
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var mc = new MapperConfiguration(cfg => cfg.AddExpressionMapping().AddProfile(new MappingProfile()));
            services.AddAutoMapper(cfg => cfg.AddExpressionMapping().AddProfile(new MappingProfile()), Assembly.GetExecutingAssembly());
            services.AddSingleton(mc);
            return services;
        }

        /// <summary>
        /// mvc
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMyMvc(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; // 设置时区为 UTC
            }).AddXmlDataContractSerializerFormatters().AddControllersAsServices().AddViewComponentsAsServices().AddTagHelpersAsServices(); // MVC
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            }); //解决razor视图中中文被编码的问题
            return services;
        }

        /// <summary>
        /// 输出缓存
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddResponseCache(this IServiceCollection services)
        {
            services.AddResponseCaching(); //注入响应缓存
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            }).Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            }).AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "text/html; charset=utf-8",
                    "application/xhtml+xml",
                    "application/atom+xml",
                    "image/svg+xml"
                });
            });
            return services;
        }

        /// <summary>
        /// 添加静态资源打包
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBundles(this IApplicationBuilder app)
        {
            app.UseBundling(bundles =>
            {
                bundles.AddCss("/main.css")
                    .Include("/fonts/icomoon.min.css")
                    .Include("/Content/jquery.paging.css")
                    .Include("/Content/common/reset.css")
                    .Include("/Content/common/loading.css")
                    .Include("/Content/common/style.css")
                    .Include("/Content/common/articlestyle.css")
                    .Include("/Content/common/leaderboard.css")
                    .Include("/Content/microtip.min.css")
                    .Include("/Assets/breadcrumb/style.css")
                    .Include("/Assets/nav/css/style.css");
                bundles.AddCss("/filemanager.css")
                    .Include("/Content/bootswatch.min.css")
                    .Include("/fonts/icomoon.min.css")
                    .Include("/ng-views/filemanager/css/animations.css")
                    .Include("/ng-views/filemanager/css/dialogs.css")
                    .Include("/ng-views/filemanager/css/main.css")
                    .Include("/Content/common/loading.min.css");
                bundles.AddCss("/dashboard.css")
                    .Include("/fonts/icomoon.min.css")
                    .Include("/Assets/jedate/jedate.css")
                    .Include("/Assets/fileupload/filestyle.min.css")
                    .Include("/Content/ng-table.min.css")
                    .Include("/Content/common/loading.min.css")
                    .Include("/Content/microtip.min.css")
                    .Include("/Content/checkbox.min.css")
                    .Include("/ng-views/css/app.css");
                bundles.AddCss("/article.css")
                    .Include("/Assets/jquery.tocify/jquery.tocify.css")
                    .Include("/Assets/UEditor/third-party/SyntaxHighlighter/styles/shCore.min.css")
                    .Include("/Assets/highlight/css/highlight.css");

                bundles.AddJs("/main.js")
                    .Include("/Scripts/bootstrap-suggest.min.js")
                    .Include("/Scripts/jquery.query.js")
                    .Include("/Scripts/jquery.paging.js")
                    .Include("/Scripts/ripplet.js")
                    .Include("/Scripts/global/scripts.js")
                    .Include("/Scripts/platform.js")
                    .Include("/Assets/newsbox/jquery.bootstrap.newsbox.js")
                    .Include("/Assets/tagcloud/js/tagcloud.js")
                    .Include("/Assets/scrolltop/js/scrolltop.js")
                    .Include("/Assets/nav/js/main.js");
                bundles.AddJs("/filemanager.js")
                    .Include("/Scripts/ng-file-upload.min.js")
                    .Include("/ng-views/filemanager/js/app.js")
                    .Include("/ng-views/filemanager/js/directives/directives.js")
                    .Include("/ng-views/filemanager/js/filters/filters.js")
                    .Include("/ng-views/filemanager/js/providers/config.js")
                    .Include("/ng-views/filemanager/js/entities/chmod.js")
                    .Include("/ng-views/filemanager/js/entities/item.js")
                    .Include("/ng-views/filemanager/js/services/apihandler.js")
                    .Include("/ng-views/filemanager/js/services/apimiddleware.js")
                    .Include("/ng-views/filemanager/js/services/filenavigator.js")
                    .Include("/ng-views/filemanager/js/providers/translations.js")
                    .Include("/ng-views/filemanager/js/controllers/main.js")
                    .Include("/ng-views/filemanager/js/controllers/selector-controller.js");
                bundles.AddJs("/article.js")
                    .Include("/Assets/highlight/js/highlight.js")
                    .Include("/Assets/UEditor/third-party/SyntaxHighlighter/scripts/shCore.min.js")
                    .Include("/Assets/UEditor/third-party/SyntaxHighlighter/scripts/bundle.min.js")
                    .Include("/Assets/jquery.tocify/jquery.tocify.js")
                    .Include("/Scripts/global/article.js")
                    .Include("/Assets/highlight/js/highlight.js");
            });
            return app;
        }
    }
}