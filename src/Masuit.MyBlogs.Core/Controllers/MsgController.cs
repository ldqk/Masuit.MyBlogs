using CacheManager.Core;
using Dispose.Scope;
using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.Html;
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 留言板和站内信
/// </summary>
public sealed class MsgController : BaseController
{
	/// <summary>
	/// 留言
	/// </summary>
	public ILeaveMessageService LeaveMessageService { get; set; }

	/// <summary>
	/// 站内信
	/// </summary>
	public IInternalMessageService MessageService { get; set; }

	public IWebHostEnvironment HostEnvironment { get; set; }

	public ICacheManager<int> MsgFeq { get; set; }

	/// <summary>
	/// 留言板
	/// </summary>
	/// <returns></returns>
	[Route("msg"), Route("msg/{cid:int}"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
	public async Task<ActionResult> Index()
	{
		ViewBag.TotalCount = LeaveMessageService.Count(m => m.ParentId == null && m.Status == Status.Published);
		var text = await new FileInfo(Path.Combine(HostEnvironment.WebRootPath, "template", "agreement.html")).ShareReadWrite().ReadAllTextAsync(Encoding.UTF8);
		return CurrentUser.IsAdmin ? View("Index_Admin", text) : View(model: text);
	}

	/// <summary>
	/// 获取留言
	/// </summary>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <param name="cid"></param>
	/// <returns></returns>
	public async Task<ActionResult> GetMsgs([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15, int? cid = null)
	{
		if (cid > 0)
		{
			var message = await LeaveMessageService.GetByIdAsync(cid.Value) ?? throw new NotFoundException("留言未找到");
			var layer = LeaveMessageService.GetQueryNoTracking(e => e.GroupTag == message.GroupTag).ToPooledListScope();
			foreach (var m in layer)
			{
				m.PostDate = m.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
				if (!CurrentUser.IsAdmin)
				{
					m.Email = null;
					m.IP = null;
					m.Location = null;
				}
			}

			return ResultData(new
			{
				total = 1,
				parentTotal = 1,
				page,
				size,
				rows = layer.ToTree(e => e.Id, e => e.ParentId).Mapper<IList<LeaveMessageViewModel>>()
			});
		}

		var parent = await LeaveMessageService.GetPagesAsync(page, size, m => m.ParentId == null && (m.Status == Status.Published || CurrentUser.IsAdmin), m => m.PostDate, false);
		if (!parent.Data.Any())
		{
			return ResultData(null, false, "没有留言");
		}
		var total = parent.TotalCount;
		var tags = parent.Data.Select(c => c.GroupTag).ToArray();
		var messages = LeaveMessageService.GetQueryNoTracking(c => tags.Contains(c.GroupTag)).ToPooledListScope();
		messages.ForEach(m =>
		{
			m.PostDate = m.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
			if (!CurrentUser.IsAdmin)
			{
				m.Email = null;
				m.IP = null;
				m.Location = null;
			}
		});
		if (total > 0)
		{
			return ResultData(new
			{
				total,
				parentTotal = total,
				page,
				size,
				rows = messages.OrderByDescending(c => c.PostDate).ToTree(c => c.Id, c => c.ParentId).Mapper<IList<LeaveMessageViewModel>>()
			});
		}

		return ResultData(null, false, "没有留言");
	}

	/// <summary>
	/// 发表留言
	/// </summary>
	/// <param name="mailSender"></param>
	/// <param name="cmd"></param>
	/// <returns></returns>
	[HttpPost, ValidateAntiForgeryToken]
	public async Task<ActionResult> Submit([FromServices] IMailSender mailSender, LeaveMessageCommand cmd)
	{
		var match = Regex.Match(cmd.NickName + cmd.Content.RemoveHtmlTag(), CommonHelper.BanRegex);
		if (match.Success)
		{
			LogManager.Info($"提交内容：{cmd.NickName}/{cmd.Content}，敏感词：{match.Value}");
			return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请检查您的内容后尝试重新提交！");
		}

		var error = ValidateEmailCode(mailSender, cmd.Email, cmd.Code);
		if (!string.IsNullOrEmpty(error))
		{
			return ResultData(null, false, error);
		}

		if (cmd.ParentId > 0 && DateTime.Now - LeaveMessageService[cmd.ParentId.Value, m => m.PostDate] > TimeSpan.FromDays(180))
		{
			return ResultData(null, false, "当前留言过于久远，不再允许回复！");
		}

		cmd.Content = cmd.Content.Trim().Replace("<p><br></p>", string.Empty);
		var ip = ClientIP.ToString();
		if (MsgFeq.GetOrAdd("Comments:" + ip, 1) > 2)
		{
			MsgFeq.Expire("Comments:" + ip, TimeSpan.FromMinutes(1));
			return ResultData(null, false, "您的发言频率过快，请稍后再发表吧！");
		}

		var msg = cmd.Mapper<LeaveMessage>();
		if (cmd.ParentId > 0)
		{
			msg.GroupTag = LeaveMessageService.GetQuery(c => c.Id == cmd.ParentId).Select(c => c.GroupTag).FirstOrDefault();
			msg.Path = (LeaveMessageService.GetQuery(c => c.Id == cmd.ParentId).Select(c => c.Path).FirstOrDefault() + "," + cmd.ParentId).Trim(',');
		}
		else
		{
			msg.GroupTag = SnowFlake.NewId;
			msg.Path = SnowFlake.NewId;
		}

		if (Regex.Match(cmd.NickName + cmd.Content, CommonHelper.ModRegex).Length <= 0)
		{
			msg.Status = Status.Published;
		}

		msg.PostDate = DateTime.Now;
		var user = HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
		if (user != null)
		{
			msg.NickName = user.NickName;
			msg.Email = user.Email;
			if (user.IsAdmin)
			{
				msg.Status = Status.Published;
				msg.IsMaster = true;
			}
		}

		msg.Content = await cmd.Content.HtmlSantinizerStandard().ClearImgAttributes();
		msg.Browser = cmd.Browser ?? Request.Headers[HeaderNames.UserAgent];
		msg.IP = ip;
		msg.Location = Request.Location();
		msg = LeaveMessageService.AddEntitySaved(msg);
		if (msg == null)
		{
			return ResultData(null, false, "留言发表失败！");
		}

		Response.Cookies.Append("NickName", msg.NickName, new CookieOptions()
		{
			Expires = DateTimeOffset.Now.AddYears(1),
			SameSite = SameSiteMode.Lax
		});
		WriteEmailKeyCookie(cmd.Email);
		MsgFeq.AddOrUpdate("Comments:" + ip, 1, i => i + 1, 5);
		MsgFeq.Expire("Comments:" + ip, TimeSpan.FromMinutes(1));
		var email = CommonHelper.SystemSettings["ReceiveEmail"];
		var content = new Template(await new FileInfo(HostEnvironment.WebRootPath + "/template/notify.html").ShareReadWrite().ReadAllTextAsync(Encoding.UTF8)).Set("title", "网站留言板").Set("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Set("nickname", msg.NickName).Set("content", msg.Content);
		if (msg.Status == Status.Published)
		{
			if (!msg.IsMaster)
			{
				await MessageService.AddEntitySavedAsync(new InternalMessage()
				{
					Title = $"来自【{msg.NickName}】的新留言",
					Content = msg.Content,
					Link = Url.Action("Index", "Msg", new { cid = msg.Id })
				});
			}
			if (msg.ParentId == null)
			{
				//新评论，只通知博主
				BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "|博客新留言：", content.Set("link", Url.Action("Index", "Msg", new { cid = msg.Id }, Request.Scheme)).Render(false), email, ip));
			}
			else
			{
				//通知博主和上层所有关联的评论访客
				var emails = LeaveMessageService.GetQuery(e => e.GroupTag == msg.GroupTag).Select(c => c.Email).Distinct().AsEnumerable().Append(email).Except(new[] { msg.Email }).ToHashSet();
				string link = Url.Action("Index", "Msg", new { cid = msg.Id }, Request.Scheme);
				foreach (var s in emails)
				{
					BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{Request.Host}{CommonHelper.SystemSettings["Title"]} 留言回复：", content.Set("link", link).Render(false), s, ip));
				}
			}
			return ResultData(null, true, "留言发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将会显示到列表中！");
		}

		BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "|博客新留言(待审核)：", content.Set("link", Url.Action("Index", "Msg", new
		{
			cid = msg.Id
		}, Request.Scheme)).Render(false) + "<p style='color:red;'>(待审核)</p>", email, ip));
		return ResultData(null, true, "留言发表成功，待站长审核通过以后将显示到列表中！");
	}

	/// <summary>
	/// 审核
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Pass(int id)
	{
		var msg = await LeaveMessageService.GetByIdAsync(id);
		msg.Status = Status.Published;
		bool b = await LeaveMessageService.SaveChangesAsync() > 0;
		if (b)
		{
			var content = new Template(await new FileInfo(Path.Combine(HostEnvironment.WebRootPath, "template", "notify.html")).ShareReadWrite().ReadAllTextAsync(Encoding.UTF8)).Set("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Set("nickname", msg.NickName).Set("content", msg.Content);
			var emails = LeaveMessageService.GetQuery(m => m.GroupTag == msg.GroupTag).Select(m => m.Email).Distinct().AsEnumerable().Except(new List<string> { msg.Email, CurrentUser.Email }).ToPooledSetScope();
			var link = Url.Action("Index", "Msg", new { cid = id }, Request.Scheme);
			foreach (var s in emails)
			{
				BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{Request.Host}{CommonHelper.SystemSettings["Title"]} 留言回复：", content.Set("link", link).Render(false), s, ClientIP.ToString()));
			}
		}

		return ResultData(null, b, b ? "审核通过！" : "审核失败！");
	}

	/// <summary>
	/// 删除留言
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public ActionResult Delete(int id)
	{
		var b = LeaveMessageService.DeleteById(id);
		return ResultData(null, b, b ? "删除成功！" : "删除失败！");
	}

	/// <summary>
	/// 获取待审核的留言
	/// </summary>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> GetPendingMsgs([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
	{
		var list = await LeaveMessageService.GetPagesAsync<DateTime, LeaveMessageDto>(page, size, m => m.Status == Status.Pending, l => l.PostDate, false);
		foreach (var m in list.Data)
		{
			m.PostDate = m.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
		}

		return Ok(list);
	}

	#region 站内消息

	/// <summary>
	/// 已读站内信
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Read(int id)
	{
		await MessageService.GetQuery(m => m.Id == id).ExecuteUpdateAsync(s => s.SetProperty(m => m.Read, true));
		return Content("ok");
	}

	/// <summary>
	/// 标记为未读
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Unread(int id)
	{
		await MessageService.GetQuery(m => m.Id == id).ExecuteUpdateAsync(s => s.SetProperty(m => m.Read, false));
		return Content("ok");
	}

	/// <summary>
	/// 标记为已读
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> MarkRead(int id)
	{
		await MessageService.GetQuery(m => m.Id <= id).ExecuteUpdateAsync(s => s.SetProperty(m => m.Read, true));
		return ResultData(null);
	}

	/// <summary>
	/// 删除站内信
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> DeleteMsg(int id)
	{
		bool b = await MessageService.DeleteByIdAsync(id) > 0;
		return ResultData(null, b, b ? "站内消息删除成功！" : "站内消息删除失败！");
	}

	/// <summary>
	/// 获取站内信
	/// </summary>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <returns></returns>
	[MyAuthorize]
	public ActionResult GetInternalMsgs([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
	{
		var msgs = MessageService.GetPagesNoTracking(page, size, m => true, m => m.Time, false);
		return Ok(msgs);
	}

	/// <summary>
	/// 获取未读消息
	/// </summary>
	/// <returns></returns>
	[MyAuthorize]
	public ActionResult GetUnreadMsgs()
	{
		var msgs = MessageService.GetQueryNoTracking(m => !m.Read, m => m.Time, false).ToPooledListScope();
		return ResultData(msgs);
	}

	/// <summary>
	/// 清除站内信
	/// </summary>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> ClearMsgs()
	{
		await MessageService.DeleteEntitySavedAsync(m => m.Read);
		return ResultData(null, true, "站内消息清除成功！");
	}

	#endregion 站内消息
}