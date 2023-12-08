using Hangfire;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire;

public sealed class HangfireActivator(IServiceProvider serviceProvider) : JobActivator
{
    public override object ActivateJob(Type type)
    {
        return serviceProvider.GetService(type);
    }
}
