using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Mvc;
using Polly;
using System.Text;
using Dispose.Scope;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 控制面板
/// </summary>
public sealed class DashboardController : AdminController
{
	/// <summary>
	/// 控制面板
	/// </summary>
	/// <returns></returns>
	[Route("dashboard"), ResponseCache(Duration = 60, VaryByHeader = "Cookie")]
	public ActionResult Index()
	{
		Response.Cookies.Append("lang", "zh-cn", new CookieOptions()
		{
			Expires = DateTime.Now.AddYears(1),
		});
		return View();
	}

	[Route("counter")]
	public ActionResult Counter()
	{
		return View();
	}

	/// <summary>
	/// 获取站内消息
	/// </summary>
	/// <returns></returns>
	public async Task<ActionResult> GetMessages([FromServices] IPostService postService, [FromServices] ILeaveMessageService leaveMessageService, [FromServices] ICommentService commentService, CancellationToken cancellationToken)
	{
		Response.ContentType = "text/event-stream";
		while (true)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			await Response.WriteAsync($"event: message\n", cancellationToken);
			var post = postService.GetQuery(p => p.Status == Status.Pending).Select(p => new
			{
				p.Id,
				p.Title,
				p.PostDate,
				p.Author
			}).ToPooledListScope();
			var msgs = leaveMessageService.GetQuery(m => m.Status == Status.Pending).Select(p => new
			{
				p.Id,
				p.PostDate,
				p.NickName
			}).ToPooledListScope();
			var comments = commentService.GetQuery(c => c.Status == Status.Pending).Select(p => new
			{
				p.Id,
				p.CommentDate,
				p.PostId,
				p.NickName
			}).ToPooledListScope();
			await Response.WriteAsync("data:" + new
			{
				post,
				msgs,
				comments
			}.ToJsonString() + "\r\r");
			await Response.Body.FlushAsync(cancellationToken);
			await Task.Delay(5000, cancellationToken);
		}

		Response.Body.Close();
		return ResultData(null);
	}

	/// <summary>
	/// 获取日志文件列表
	/// </summary>
	/// <returns></returns>
	public ActionResult GetLogfiles()
	{
		var files = Directory.GetFiles(LogManager.LogDirectory).OrderByDescending(s => s).Select(Path.GetFileName).ToPooledListScope();
		return ResultData(files);
	}

	/// <summary>
	/// 查看日志
	/// </summary>
	/// <param name="filename"></param>
	/// <returns></returns>
	public ActionResult Catlog([FromBodyOrDefault] string filename)
	{
		if (System.IO.File.Exists(Path.Combine(LogManager.LogDirectory, filename)))
		{
			string text = new FileInfo(Path.Combine(LogManager.LogDirectory, filename)).ShareReadWrite().ReadAllText(Encoding.UTF8);
			return ResultData(text);
		}
		return ResultData(null, false, "文件不存在！");
	}

	/// <summary>
	/// 删除文件
	/// </summary>
	/// <param name="filename"></param>
	/// <returns></returns>
	public ActionResult DeleteFile([FromBodyOrDefault] string filename)
	{
		Policy.Handle<IOException>().WaitAndRetry(5, i => TimeSpan.FromSeconds(1)).Execute(() => System.IO.File.Delete(Path.Combine(LogManager.LogDirectory, filename)));
		return ResultData(null, message: "文件删除成功!");
	}

	/// <summary>
	/// 资源管理器
	/// </summary>
	/// <returns></returns>
	[Route("filemanager"), ResponseCache(Duration = 60, VaryByHeader = "Cookie")]
	public ActionResult FileManager()
	{
		return View();
	}
}
