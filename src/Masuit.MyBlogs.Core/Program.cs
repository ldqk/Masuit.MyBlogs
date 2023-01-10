using Autofac.Extensions.DependencyInjection;
using Masuit.MyBlogs.Core;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Infrastructure.Drive;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Z.EntityFramework.Plus;

QueryCacheManager.DefaultMemoryCacheEntryOptions = new MemoryCacheEntryOptions()
{
	AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
};
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (Environment.OSVersion.Platform is not (PlatformID.MacOSX or PlatformID.Unix))
{
	// 设置相关进程优先级为高于正常，防止其他进程影响应用程序的运行性能
	Process.GetProcessesByName("mysqld").ForEach(p => p.PriorityClass = ProcessPriorityClass.AboveNormal);
	Process.GetProcessesByName("pg_ctl").ForEach(p => p.PriorityClass = ProcessPriorityClass.AboveNormal);
	Process.GetProcessesByName("postgres").ForEach(p => p.PriorityClass = ProcessPriorityClass.AboveNormal);
	Process.GetProcessesByName("redis-server").ForEach(p => p.PriorityClass = ProcessPriorityClass.AboveNormal);
	Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
}

// 确保IP数据库正常
if (!"223.5.5.5".GetIPLocation().Contains("阿里"))
{
	throw new Exception("IP地址库初始化失败，请重启应用！");
}

InitOneDrive(); // 初始化Onedrive程序
Host.CreateDefaultBuilder(args).UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureWebHostDefaults(hostBuilder => hostBuilder.UseKestrel(opt =>
{
	var config = opt.ApplicationServices.GetService<IConfiguration>();
	var port = config["Port"] ?? "5000";
	var sslport = config["Https:Port"] ?? "5001";
	opt.ListenAnyIP(port.ToInt32(), options => options.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
	if (bool.Parse(config["Https:Enabled"]))
	{
		opt.ListenAnyIP(sslport.ToInt32(), s =>
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 10)
			{
				s.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
			}

			s.UseHttps(AppContext.BaseDirectory + config["Https:CertPath"], config["Https:CertPassword"]);
		});
	}

	opt.Limits.MaxRequestBodySize = null;
	Console.WriteLine($"应用程序监听端口：http：{port}，https：{sslport}");
}).UseStartup<Startup>()).Build().Run();

static void InitOneDrive()
{
	//初始化
	if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "OneDrive.db")))
	{
		File.Copy(Path.Combine("App_Data", "OneDrive.template.db"), Path.Combine("App_Data", "OneDrive.db"));
		Console.WriteLine("数据库创建成功");
	}

	using var settingService = new SettingService(new DriveContext());
	if (settingService.Get("IsInit") != "true")
	{
		settingService.Set("IsInit", "true").Wait();
		Console.WriteLine("数据初始化成功");
		Console.WriteLine($"请登录 {OneDriveConfiguration.BaseUri}/#/admin 进行身份及其他配置");
	}
}
