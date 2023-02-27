using Hangfire;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire;

public sealed class HangfireActivator : JobActivator
{
	private readonly IServiceProvider _serviceProvider;

	public HangfireActivator(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public override object ActivateJob(Type type)
	{
		return _serviceProvider.GetService(type);
	}
}
