using Autofac.Extensions.DependencyInjection;
using Masuit.MyBlogs.Core.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace Masuit.MyBlogs.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MyHub.Init();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = new ConfigurationBuilder().AddCommandLine(args).SetBasePath(Directory.GetCurrentDirectory()).AddEnvironmentVariables().AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();
            var port = config["port"] ?? Environment.GetEnvironmentVariable("port") ?? "5000";
            var sslport = config["sslport"] ?? Environment.GetEnvironmentVariable("sslport") ?? "5001";
            return Host.CreateDefaultBuilder(args).UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureWebHostDefaults(configurationBuilder =>
            {
                configurationBuilder.UseKestrel(opt =>
                {
                    opt.ListenAnyIP(port.ToInt32());
                    if (bool.Parse(config["Https:Enabled"]))
                    {
                        opt.ListenAnyIP(sslport.ToInt32(), s =>
                        {
                            s.UseHttps(AppContext.BaseDirectory + config["Https:CertPath"], config["Https:CertPassword"]);
                        });
                    }

                    opt.Limits.MaxRequestBodySize = null;
                }).UseIISIntegration().UseStartup<Startup>();
            });
        }
    }
}