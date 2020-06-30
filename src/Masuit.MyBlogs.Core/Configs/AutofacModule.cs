using Autofac;
using Hangfire;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using System.Diagnostics;
using System.Reflection;

namespace Masuit.MyBlogs.Core.Configs
{
    public class AutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsImplementedInterfaces().Where(t => t.Name.EndsWith("Repository") || t.Name.EndsWith("Service") || t.Name.EndsWith("Controller") || t.Name.EndsWith("Attribute")).PropertiesAutowired().AsSelf().InstancePerDependency();
            builder.RegisterType<BackgroundJobClient>().SingleInstance();
            builder.RegisterType<FirewallAttribute>().PropertiesAutowired().AsSelf().InstancePerDependency();
            builder.RegisterType<HangfireBackJob>().As<IHangfireBackJob>().PropertiesAutowired(PropertyWiringOptions.PreserveSetValues).InstancePerDependency();
        }
    }
}