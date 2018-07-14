using System.Web.Mvc;
using Common;
using FluentScheduler;
using Models.Application;

namespace Masuit.MyBlogs.WebApp
{
    /// <summary>
    /// 网站启动配置
    /// </summary>
    public class StartupConfig
    {
        public static void Startup()
        {
            //移除aspx视图引擎
            ViewEngines.Engines.RemoveAt(0);
            RegisterAutomapper.Excute();
            using (new DataContext()) { }
            HangfireConfig.Register();
            Registry reg = new Registry();
            reg.Schedule(() => CollectRunningInfo.Start()).ToRunNow().AndEvery(5).Seconds();//启动服务器监控任务
            JobManager.Initialize(reg);//初始化定时器
        }
    }
}