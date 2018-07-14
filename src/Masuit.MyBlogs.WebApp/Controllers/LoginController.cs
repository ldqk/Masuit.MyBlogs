using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IBLL;
using Models.DTO;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    [RoutePrefix("login")]
    public class LoginController : AdminController
    {
        public ILoginRecordBll LoginRecordBll { get; set; }

        public LoginController(ILoginRecordBll loginRecordBll)
        {
            LoginRecordBll = loginRecordBll;
        }

        [Route("delete/{id:int}/{ids}")]
        public ActionResult Delete(int id, string ids)
        {
            if (!string.IsNullOrWhiteSpace(ids))
            {
                bool b = LoginRecordBll.DeleteEntitySaved(r => r.UserInfoId == id && ids.Contains(r.Id.ToString())) > 0;
                return ResultData(null, b, b ? "删除成功！" : "删除失败");
            }
            return ResultData(null, false, "数据不合法");
        }

        [Route("getrecent/{id:int}")]
        public ActionResult GetRecentRecord(int id)
        {
            var time = DateTime.Now.AddMonths(-1);
            List<LoginRecordOutputDto> list = LoginRecordBll.LoadEntitiesFromL2CacheNoTracking<DateTime, LoginRecordOutputDto>(r => r.UserInfoId == id && r.LoginTime >= time, r => r.LoginTime, false).ToList();
            return ResultData(list);
        }
    }
}