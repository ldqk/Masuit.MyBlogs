using Masuit.Tools.Logging;

namespace Masuit.MyBlogs.Core.Common;

public abstract class ScheduledService : IHostedService, IDisposable
{
    private readonly Timer _timer;
    private readonly TimeSpan _period;

    protected ScheduledService(TimeSpan period)
    {
        _period = period;
        _timer = new Timer(Execute, null, Timeout.Infinite, 0);
    }

    public void Execute(object state = null)
    {
        try
        {
            ExecuteAsync().Wait();
        }
        catch (Exception ex)
        {
            LogManager.Error(ex);
        }
    }

    protected abstract Task ExecuteAsync();

    public virtual void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Change(TimeSpan.FromSeconds(Random.Shared.Next(10)), _period);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}
