using Masuit.MyBlogs.Core.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public interface IRequestLogger
{
	void Log(string ip, string url, string userAgent, string traceid);

	void Process();
}

public class RequestNoneLogger : IRequestLogger
{
	public void Log(string ip, string url, string userAgent, string traceid)
	{
	}

	public void Process()
	{
	}
}

public class RequestFileLogger : IRequestLogger
{
	public void Log(string ip, string url, string userAgent, string traceid)
	{
		TrackData.RequestLogs.AddOrUpdate(ip, new RequestLog
		{
			Count = 1,
			RequestUrls =
			{
				url
			},
			UserAgents =
			{
				userAgent
			}
		}, (_, i) =>
		{
			i.UserAgents.Add(userAgent);
			i.RequestUrls.Add(url);
			i.Count++;
			return i;
		});
	}

	public void Process()
	{
	}
}

public class RequestDatabaseLogger : IRequestLogger
{
	private static readonly ConcurrentQueue<RequestLogDetail> Queue = new();
	private readonly LoggerDbContext _dataContext;

	public RequestDatabaseLogger(LoggerDbContext dataContext)
	{
		_dataContext = dataContext;
	}

	public void Log(string ip, string url, string userAgent, string traceid)
	{
		Queue.Enqueue(new RequestLogDetail
		{
			Time = DateTime.Now,
			UserAgent = userAgent,
			RequestUrl = url,
			IP = ip,
			TraceId = traceid
		});
	}

	public void Process()
	{
		if (Debugger.IsAttached)
		{
			return;
		}

		while (Queue.TryDequeue(out var result))
		{
			var location = result.IP.GetIPLocation();
			result.Location = location;
			result.Country = new[]
			{
				location.Country,
				location.Address2.Country
			}.Where(s => !string.IsNullOrEmpty(s)).Distinct().Join("|");
			result.City = new[]
			{
				location.City,
				location.Address2.City
			}.Where(s => !string.IsNullOrEmpty(s)).Distinct().Join("|");
			result.Network = location.Network;
			_dataContext.Add(result);
		}

		if (_dataContext.SaveChanges() > 0)
		{
			var start = DateTime.Now.AddMonths(-6);
			_dataContext.Set<RequestLogDetail>().Where(e => e.Time < start).DeleteFromQuery();
		}
	}
}

public class RequestLoggerBackService : ScheduledService
{
	private readonly IServiceScopeFactory _scopeFactory;

	public RequestLoggerBackService(IServiceScopeFactory scopeFactory) : base(TimeSpan.FromMinutes(5))
	{
		_scopeFactory = scopeFactory;
	}

	protected override Task ExecuteAsync()
	{
#if RELEASE
		using var scope = _scopeFactory.CreateAsyncScope();
		var logger = scope.ServiceProvider.GetRequiredService<IRequestLogger>();
		logger.Process();
#endif
		return Task.CompletedTask;
	}
}

public static class RequestLoggerServiceExtension
{
	public static IServiceCollection AddRequestLogger(this IServiceCollection services, IConfiguration configuration)
	{
		switch (configuration["RequestLogStorage"])
		{
			case "database":
				services.AddScoped<IRequestLogger, RequestDatabaseLogger>();
				services.TryAddScoped<RequestDatabaseLogger>();
				break;

			case "file":
				services.AddSingleton<IRequestLogger, RequestFileLogger>();
				break;

			default:
				services.AddSingleton<IRequestLogger, RequestNoneLogger>();
				break;
		}

		services.AddHostedService<RequestLoggerBackService>();
		return services;
	}
}