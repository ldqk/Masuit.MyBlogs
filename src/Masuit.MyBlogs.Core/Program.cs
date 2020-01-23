using Autofac.Extensions.DependencyInjection;
using Masuit.MyBlogs.Core.Hubs;
using Masuit.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Masuit.MyBlogs.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MyHub.Init();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureWebHostDefaults(hostBuilder => hostBuilder.UseKestrel(opt =>
        {
            var config = opt.ApplicationServices.GetService<IConfiguration>();
            var port = config["Port"] ?? "5000";
            opt.ListenAnyIP(port.ToInt32());
            if (bool.Parse(config["Https:Enabled"]))
            {
                opt.ListenAnyIP(config["Https:Port"].ToInt32(), s =>
                {
                    s.UseHttps(AppContext.BaseDirectory + config["Https:CertPath"], config["Https:CertPassword"]);
                });
            }
            opt.Limits.MaxRequestBodySize = null;
        }).UseIISIntegration().UseStartup<Startup>());
    }
}