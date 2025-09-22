using Dispose.Scope;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.AspNetCore.ModelBinder;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 快速分享
/// </summary>
public sealed class ShareController : AdminController
{
    /// <summary>
    /// 快速分享
    /// </summary>
    public IFastShareService FastShareService { get; set; }

    /// <summary>
    /// 快速分享
    /// </summary>
    /// <returns></returns>
    public ActionResult Index()
    {
        var shares = FastShareService.GetAll(s => s.Sort).ToPooledListScope();
        return ResultData(shares);
    }

    /// <summary>
    /// 添加快速分享
    /// </summary>
    /// <param name="share"></param>
    /// <returns></returns>
    [HttpPost, DistributedLockFilter]
    public ActionResult Add([FromBodyOrDefault] FastShare share)
    {
        bool b = FastShareService.AddEntitySaved(share) != null;
        return ResultData(null, b, b ? "添加成功" : "添加失败");
    }

    /// <summary>
    /// 移除快速分享
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("/share/{id}"), DistributedLockFilter]
    public async Task<ActionResult> Remove(int id)
    {
        bool b = await FastShareService.DeleteByIdAsync(id) > 0;
        return ResultData(null, b, b ? "删除成功" : "删除失败");
    }

    /// <summary>
    /// 更新快速分享
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost, DistributedLockFilter]
    public async Task<ActionResult> Update([FromBodyOrDefault] FastShare model)
    {
        var b = await FastShareService.GetQuery(s => s.Id == model.Id).ExecuteUpdateAsync(s => s.SetProperty(e => e.Title, model.Title).SetProperty(e => e.Link, model.Link).SetProperty(e => e.Sort, model.Sort)) > 0;
        return ResultData(null, b, b ? "更新成功" : "更新失败");
    }
}