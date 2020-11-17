using Autofac.Extensions.DependencyInjection;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Hubs;
using Masuit.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Z.EntityFramework.Plus;

namespace Masuit.MyBlogs.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!"114.114.114.114".GetIPLocation().Contains("南京市"))
            {
                throw new Exception("IP地址库初始化失败，请重启应用！");
            }

            QueryCacheManager.DefaultMemoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            MyHub.Init();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureWebHostDefaults(hostBuilder => hostBuilder.UseKestrel(opt =>
        {
            var config = opt.ApplicationServices.GetService<IConfiguration>();
            var port = config["Port"] ?? "5000";
            var sslport = config["Https:Port"] ?? "5001";
            opt.ListenAnyIP(port.ToInt32());
            if (bool.Parse(config["Https:Enabled"]))
            {
                opt.ListenAnyIP(sslport.ToInt32(), s => s.UseHttps(AppContext.BaseDirectory + config["Https:CertPath"], config["Https:CertPassword"]));
            }

            opt.Limits.MaxRequestBodySize = null;
            Console.WriteLine($"应用程序监听端口：http：{port}，https：{sslport}");
        }).UseStartup<Startup>());
    }
}