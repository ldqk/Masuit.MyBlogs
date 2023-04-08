namespace Masuit.MyBlogs.Core.Common;

public class RequestLog
{
    public ConcurrentHashSet<string> UserAgents { get; } = new();

    public ConcurrentHashSet<string> RequestUrls { get; } = new();

    public int Count { get; set; }
}
