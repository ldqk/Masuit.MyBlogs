using Masuit.Tools.Mime;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Masuit.MyBlogs.Core.Extensions;

public static class MiddlewareExtension
{
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
            options.Filters.Add<ExceptionFilter>();
            options.Filters.Add<PerfCounterFilterAttribute>();
        }).AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
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
            options.Level = CompressionLevel.Fastest;
        }).Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        }).AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([
                "text/html; charset=utf-8",
                "text/event-stream",
                "application/xhtml+xml",
                "application/atom+xml",
                "image/svg+xml"
            ]);
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
                .Include("/fonts/icomoon.css")
                .Include("/Content/common/reset.css")
                .Include("/Content/common/style.css")
                .Include("/Assets/nav/css/style.css");
            bundles.AddCss("/filemanager.css")
                .Include("/Content/bootswatch.min.css")
                .Include("/fonts/icomoon.css")
                .Include("/ng-views/filemanager/css/animations.css")
                .Include("/ng-views/filemanager/css/dialogs.css")
                .Include("/ng-views/filemanager/css/main.css");
            bundles.AddCss("/article.css")
                .Include("/Assets/auto-toc/auto-toc.css")
                .Include("/UEditorPlus/third-party/SyntaxHighlighter/shCoreDefault.css");

            bundles.AddJs("/main.js")
                .Include("/Scripts/ripplet.js")
                .Include("/Scripts/global/functions.js")
                .Include("/Scripts/global/scripts.js")
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
                .Include("/UEditorPlus/third-party/SyntaxHighlighter/shCore.js")
                .Include("/Assets/auto-toc/auto-toc.js")
                .Include("/Scripts/global/article.js");
        });
        return app;
    }
}

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is NotFoundException)
        {
            context.HttpContext.Response.StatusCode = 404;
            string accept = context.HttpContext.Request.Headers[HeaderNames.Accept] + "";
            context.Result = true switch
            {
                _ when accept.StartsWith("image") => new VirtualFileResult("/Assets/images/404/4044.jpg", ContentType.Jpeg),
                _ when context.HttpContext.Request.HasJsonContentType() || context.HttpContext.Request.Method == HttpMethods.Post => new JsonResult(new
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "页面未找到！"
                }),
                _ => new ViewResult()
                {
                    ViewName = "/Views/Error/Index.cshtml"
                }
            };
            context.ExceptionHandled = true;
        }
    }
}