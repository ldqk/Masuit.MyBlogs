using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    [Route("values")]
    public class ValuesController : AdminController
    {
        public IVariablesService VariablesService { get; set; }

        [HttpGet("list")]
        public async Task<ActionResult> GetAll()
        {
            return ResultData(await VariablesService.GetAll().ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult> Save(Variables model)
        {
            var b = await VariablesService.AddOrUpdateSavedAsync(v => v.Key, model) > 0;
            return ResultData(null, b, b ? "保存成功" : "保存失败");
        }

        [HttpPost("{id:int}")]
        public ActionResult Delete(int id)
        {
            var b = VariablesService - id;
            return ResultData(null, b, b ? "删除成功" : "保存失败");
        }
    }
}