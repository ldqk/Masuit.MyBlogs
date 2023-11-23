namespace Masuit.MyBlogs.Core.Extensions.UEditor;

/// <summary>
/// NotSupportedHandler 的摘要说明
/// </summary>
public class NotSupportedHandler(HttpContext context) : Handler(context)
{
    public override Task<string> Process()
    {
        return Task.FromResult(WriteJson(new
        {
            state = "action 参数为空或者 action 不被支持。"
        }));
    }
}
