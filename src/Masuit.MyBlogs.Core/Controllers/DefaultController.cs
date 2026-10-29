using FreeRedis;
using Masuit.Tools.DateTimeExt;
using System.Diagnostics;
using System.Globalization;

namespace Masuit.MyBlogs.Core.Controllers;

public sealed class DefaultController(IRedisClient redis) : Controller
{
    /// <summary>
    /// 在线状态和延迟探测
    /// </summary>
    /// <returns></returns>
    [Route("/ping")]
    public async Task<IActionResult> Ping(bool probe = false, CancellationToken cancellationToken = default)
    {
        var started = Stopwatch.GetTimestamp();
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("X-Accel-Buffering", "no");
        Response.Headers.Append("Cache-Control", "no-cache");

        if (probe)
        {
            var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture);
            await Response.WriteAsync($"data: {elapsed}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            return new EmptyResult();
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await redis.SAddAsync("GlobalOnline", ip);
        while (true)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await Response.WriteAsync("event: message\n", cancellationToken);
                await Response.WriteAsync("data:" + DateTime.UtcNow.GetTotalMilliseconds() + "\r\r", cancellationToken: cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
                await Task.Delay(2000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        await redis.SRemAsync("GlobalOnline", ip);
        return new EmptyResult();
    }
}