using FreeRedis;
using Masuit.Tools.DateTimeExt;

namespace Masuit.MyBlogs.Core.Controllers;

public sealed class DefaultController(IRedisClient redis) : Controller
{
    /// <summary>
    /// 文章统计
    /// </summary>
    /// <returns></returns>
    [Route("/ping")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken = default)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("X-Accel-Buffering", "no");
        Response.Headers.Append("Cache-Control", "no-cache");
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
        return Ok();
    }
}