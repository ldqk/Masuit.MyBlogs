using Masuit.Tools.AspNetCore.ModelBinder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Masuit.MyBlogs.Core.Controllers;

[Route("values")]
public sealed class ValuesController : AdminController
{
	public IVariablesService VariablesService { get; set; }

	[HttpGet("list")]
	public async Task<ActionResult> GetAll()
	{
		return ResultData(await VariablesService.GetAllNoTracking().ToListAsync());
	}

	[HttpPost]
	public async Task<ActionResult> Save([FromBodyOrDefault] Variables model)
	{
		var b = await VariablesService.AddOrUpdateSavedAsync(v => v.Key, model) > 0;
		QueryCacheManager.ExpireType<Variables>();
		return ResultData(null, b, b ? "保存成功" : "保存失败");
	}

	[HttpPost("{id:int}")]
	public ActionResult Delete(int id)
	{
		var b = VariablesService - id;
		QueryCacheManager.ExpireType<Variables>();
		return ResultData(null, b, b ? "删除成功" : "保存失败");
	}
}