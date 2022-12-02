using Newtonsoft.Json;

namespace Masuit.MyBlogs.Core.Extensions.UEditor;

/// <summary>
/// Handler 的摘要说明
/// </summary>
public abstract class Handler
{
	protected Handler(HttpContext context)
	{
		this.Request = context.Request;
		this.Response = context.Response;
		this.Context = context;
		//this.Server = context.Server;
	}

	public abstract Task<string> Process();

	protected string WriteJson(object response)
	{
		string jsonpCallback = Request.Query["callback"];
		string json = JsonConvert.SerializeObject(response);
		return string.IsNullOrWhiteSpace(jsonpCallback) ? json : $"{jsonpCallback}({json});";
	}

	public HttpRequest Request { get; }
	public HttpResponse Response { get; }
	public HttpContext Context { get; }
}