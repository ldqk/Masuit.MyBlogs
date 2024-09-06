using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Masuit.MyBlogs.Core.Extensions;

public class PerfCounterFilterAttribute : ActionFilterAttribute
{
    public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();

    /// <inheritdoc />
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Response.Headers.IsReadOnly)
        {
            return;
        }
        context.HttpContext.Response.Headers.AddOrUpdate("X-Action-Time", Stopwatch.ElapsedMilliseconds + "ms", Stopwatch.ElapsedMilliseconds + "ms");
    }

    /// <inheritdoc />
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        Stopwatch.Restart();
        context.HttpContext.Response.OnStarting(() =>
        {
            if (context.HttpContext.Response.Headers.IsReadOnly)
            {
                return Task.CompletedTask;
            }
            context.HttpContext.Response.Headers.AddOrUpdate("X-Result-Time", Stopwatch.ElapsedMilliseconds + "ms", Stopwatch.ElapsedMilliseconds + "ms");
            return Task.CompletedTask;
        });
    }
}