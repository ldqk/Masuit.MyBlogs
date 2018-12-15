using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using BLL;
using DAL;
using Hangfire;
using Masuit.MyBlogs.WebApp.Models.Hangfire;
using Masuit.Tools.NoSQL;
using Microsoft.AspNet.SignalR;
using Models.Application;
using System.Reflection;
using System.Web.Mvc;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace Masuit.MyBlogs.WebApp
{
    /// <summary>
    /// autofac配置类
    /// </summary>
    public class AutofacConfig
    {
        public static IContainer Container { get; set; }
        /// <summary>
        /// 负责调用autofac实现依赖注入，负责创建MVC控制器类的对象(调用控制器的有参构造函数)，接管DefaultControllerFactory的工作
        /// </summary>
        public static void RegisterMVC()
        {
            //1.0 实例化autofac的创建容器
            var builder = new ContainerBuilder();

            //2.0 告诉autofac将来要创建的控制器类存放在哪个程序集
            builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);
            builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterFilterProvider();

            //3.0 告诉autofac注册所有的Bll，创建类的实例，以该类的接口实现实例存储
            builder.RegisterType<DataContext>().OnRelease(db => db.Dispose()).InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(PostDal).Assembly).AsSelf().AsImplementedInterfaces().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(PostBll).Assembly).AsSelf().AsImplementedInterfaces().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<RedisHelper>().OnRelease(db => db.Dispose()).InstancePerLifetimeScope();
            builder.RegisterType<BackgroundJobClient>().SingleInstance();//指定生命周期为单例
            builder.RegisterType<HangfireBackJob>().As<IHangfireBackJob>().PropertiesAutowired(PropertyWiringOptions.PreserveSetValues).InstancePerDependency();
            //4.0 创建一个autofac的容器
            Container = builder.Build();

            //5.0 将当前容器交给MVC底层，保证容器不被销毁，控制器由autofac来创建
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(Container);
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(Container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(Container));
        }
    }
}