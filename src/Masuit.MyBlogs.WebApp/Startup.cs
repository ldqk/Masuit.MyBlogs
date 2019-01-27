using Hangfire;
using Hangfire.Dashboard;
using Masuit.MyBlogs.WebApp;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.Net;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Models.DTO;
using Owin;
using System.Web;

[assembly: OwinStartup(typeof(Startup))]

namespace Masuit.MyBlogs.WebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //配置任务持久化到内存
            //GlobalConfiguration.Configuration.UseMemoryStorage();
            //GlobalConfiguration.Configuration.UseSqlServerStorage(ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString);
            GlobalConfiguration.Configuration.UseRedisStorage();

            //启用dashboard
            app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = 10 });
            app.UseHangfireDashboard("/taskcenter", new DashboardOptions()
            {
                Authorization = new[] { new MyRestrictiveAuthorizationFilter() }
            }); //注册dashboard的路由地址
            app.UseCors(CorsOptions.AllowAll);
            app.UseHangfireServer();
            app.MapSignalR();
        }
    }

    public class MyRestrictiveAuthorizationFilter : IDashboardAuthorizationFilter
    {
        //public RedisHelper RedisHelper { get; set; } = new RedisHelper();
        public bool Authorize(DashboardContext context)
        {
            UserInfoOutputDto user = HttpContext.Current.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            return user.IsAdmin;
        }
    }
}
