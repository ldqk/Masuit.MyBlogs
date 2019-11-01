using AutoMapper;
using CacheManager.Core;
using EFSecondLevelCache.Core;
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
            //配置EF二级缓存
            services.AddEFSecondLevelCache();
            // 配置EF二级缓存策略
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(new CacheManager.Core.ConfigurationBuilder().WithJsonSerializer().WithMicrosoftMemoryCacheHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10)).Build());
            return services;
        }

        /// <summary>
        /// automapper
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var mc = new MapperConfiguration(e => e.AddProfile(new MappingProfile()));
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
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
                options.Filters.Add<MyExceptionFilter>();
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc; // 设置时区为 UTC
            }).AddControllersAsServices().AddViewComponentsAsServices().AddTagHelpersAsServices(); // MVC
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
                options.Level = CompressionLevel.Optimal;
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
    }
}