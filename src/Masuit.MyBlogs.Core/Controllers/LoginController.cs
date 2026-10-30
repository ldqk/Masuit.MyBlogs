using Dispose.Scope;
using EFCoreSecondLevelCacheInterceptor;
using Masuit.MyBlogs.Core.Models;

namespace Masuit.MyBlogs.Core.Controllers;

[Route("login")]
public sealed class LoginController : AdminController
{
    public ILoginRecordService LoginRecordService { get; set; }

    [Route("delete/{ids}")]
    public async Task<ActionResult> Delete(string ids)
    {
        if (!string.IsNullOrWhiteSpace(ids))
        {
            bool b = await LoginRecordService.DeleteEntitySavedAsync(r => r.UserInfoId == CurrentUser.Id && ids.Contains(r.Id.ToString())) > 0;
            return ResultData(null, b, b ? "删除成功！" : "删除失败");
        }

        return ResultData(null, false, "数据不合法");
    }

    [Route("getrecent")]
    public ActionResult GetRecentRecord()
    {
        var time = DateTime.Now.AddMonths(-1);
        var list = LoginRecordService.GetQuery(r => r.UserInfoId == CurrentUser.Id && r.LoginTime >= time, r => r.LoginTime, false).ProjectViewModel().Cacheable().ToPooledListScope();
        return ResultData(list);
    }
}