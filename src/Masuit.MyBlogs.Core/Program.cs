using Autofac.Extensions.DependencyInjection;
using Masuit.MyBlogs.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Diagnostics;
using AngleSharp.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
try
{
    if (Environment.OSVersion.Platform is not (PlatformID.MacOSX or PlatformID.Unix))
    {
        // 设置相关进程优先级为高于正常，防止其他进程影响应用程序的运行性能
        Process.GetProcessesByName("pg_ctl").ForEach(p => p.PriorityClass = ProcessPriorityClass.AboveNormal);
        Process.GetProcessesByName("postgres").ForEach(p => p.PriorityClass = ProcessPriorityClass.AboveNormal);
        Process.GetProcessesByName("redis-server").ForEach(p => p.PriorityClass = ProcessPriorityClass.AboveNormal);
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
    }
}
catch
{
    // ignored
}

await Host.CreateDefaultBuilder(args).ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json", true, true)).UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureWebHostDefaults(hostBuilder => hostBuilder.UseQuic().UseKestrel(opt =>
{
    var config = opt.ApplicationServices.GetService<IConfiguration>();
    var port = config["Port"] ?? "5000";
    var sslport = config["Https:Port"] ?? "5001";
    opt.ListenAnyIP(port.ToInt32(), options => options.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
    if (config["Https:Enabled"].ToBoolean())
    {
        opt.ListenAnyIP(sslport.ToInt32(), s =>
        {
            if (Environment.OSVersion is { Platform: PlatformID.Win32NT, Version.Major: >= 10 })
            {
                s.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
            }

            s.UseHttps(AppContext.BaseDirectory + config["Https:CertPath"], config["Https:CertPassword"]);
        });
    }

    opt.Limits.MaxRequestBodySize = null;
    Console.WriteLine($"应用程序监听端口：http：{port}，https：{sslport}");
}).UseStartup<Startup>()).Build().RunAsync();