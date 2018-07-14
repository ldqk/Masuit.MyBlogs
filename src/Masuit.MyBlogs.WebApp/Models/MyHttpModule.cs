using System.Web;
using Masuit.Tools;

namespace Masuit.MyBlogs.WebApp.Models
{
    public class MyHttpModule : IHttpModule
    {
        /// <summary>初始化模块，并使其为处理请求做好准备。</summary>
        /// <param name="context">一个 <see cref="T:System.Web.HttpApplication" />，它提供对 ASP.NET 应用程序内所有应用程序对象的公用的方法、属性和事件的访问</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                if (context.Request.Url.AbsolutePath.Contains(new[] { "jpg", "png", "bmp", "gif", "" }) && (context.Request.UrlReferrer != null && !context.Request.UrlReferrer.Host.Equals(context.Request.Url.Host)))
                {
                    context.Response.WriteFile("~/favicon.ico");
                    context.Response.End();
                }
            };
        }

        /// <summary>处置由实现 <see cref="T:System.Web.IHttpModule" /> 的模块使用的资源（内存除外）。</summary>
        public void Dispose()
        {
        }
    }
}