using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Hubs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Z.EntityFramework.Extensions;

namespace Masuit.MyBlogs.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LicenseManager.AddLicense("67;100-MASUIT", "809739091397182EC1ECEA8770EB4218");
            RegisterAutomapper.Excute();
            MyHub.Init();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = new ConfigurationBuilder().AddCommandLine(args).SetBasePath(Directory.GetCurrentDirectory()).AddEnvironmentVariables().AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();
            var port = config["port"] ?? Environment.GetEnvironmentVariable("port") ?? "5000";
            var sslport = config["sslport"] ?? Environment.GetEnvironmentVariable("sslport") ?? "5001";
            return WebHost.CreateDefaultBuilder(args).UseKestrel(opt =>
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
        }
    }
}