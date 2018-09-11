using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Common;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.Net;
using Models.DTO;
using Models.Entity;
using Models.Enum;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class LinksController : BaseController
    {
        [Route("links")]
        public ActionResult Index()
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            List<LinksOutputDto> list = LinksBll.LoadEntitiesNoTracking<LinksOutputDto>(l => l.Status == Status.Available).ToList();
            if (user != null && user.IsAdmin)
            {
                return View("Index_Admin", list);
            }
            return View(list);
        }

        public async Task<ActionResult> Apply(Links links)
        {
            Uri uri = new Uri(links.Url);
            using (HttpClient client = new HttpClient()
            {
                BaseAddress = uri
            })
            {
                client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
                client.DefaultRequestHeaders.Referrer = Request.Url;
                return await await client.GetAsync(uri.PathAndQuery).ContinueWith(async t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        return ResultData(null, false, "添加失败！检测到您的网站疑似挂了，或者连接到你网站的时候超时，请检查下！");
                    }
                    var res = await t;
                    if (res.IsSuccessStatusCode)
                    {
                        var s = await res.Content.ReadAsStringAsync();
                        if (s.Contains(CommonHelper.GetSettings("Domain")))
                        {
                            bool b = LinksBll.AddOrUpdateSaved(l => l.Url, links) > 0;
                            return ResultData(null, b, b ? "添加成功！这可能有一定的延迟，如果没有看到您的链接，请稍等几分钟后刷新页面即可，如有疑问，请联系站长。" : "添加失败！这可能是由于网站服务器内部发生了错误，如有疑问，请联系站长。");
                        }
                        return ResultData(null, false, $"添加失败！检测到您的网站上未将本站设置成友情链接，请先将本站主域名：{CommonHelper.GetSettings("Domain")}在您的网站设置为友情链接，并且能够展示后，再次尝试添加即可！");
                    }
                    return ResultData(null, false, "添加失败！检测到您的网站疑似挂了！返回状态码为：" + res.StatusCode);
                });
            }
        }

        [Authority]
        public ActionResult Add(Links links)
        {
            bool b = LinksBll.AddOrUpdateSaved(l => l.Url, links) > 0;
            return b ? ResultData(null, message: "添加成功！") : ResultData(null, false, "添加失败！");
        }

        [Authority]
        public async Task<ActionResult> Check(string link)
        {
            Uri uri = new Uri(link);
            using (var client = new HttpClient()
            {
                BaseAddress = uri
            })
            {
                client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
                return await await client.GetAsync(uri.PathAndQuery).ContinueWith(async t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        return ResultData(null, false, link + " 似乎挂了！");
                    }

                    var res = await t;
                    if (res.IsSuccessStatusCode)
                    {
                        var s = await res.Content.ReadAsStringAsync();
                        if (s.Contains(CommonHelper.GetSettings("Domain")))
                        {
                            return ResultData(null, true, "友情链接正常！");
                        }
                        return ResultData(null, false, link + " 对方似乎没有本站的友情链接！");
                    }
                    return ResultData(null, false, link + " 对方网站返回错误的状态码！http响应码为：" + res.StatusCode);
                });
            }
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            bool b = LinksBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        [Authority]
        public ActionResult Edit(Links model)
        {
            Links links = LinksBll.GetById(model.Id);
            links.Name = model.Name;
            links.Url = model.Url;
            bool b = LinksBll.UpdateEntitySaved(links);
            return ResultData(null, b, b ? "保存成功" : "保存失败");
        }

        [Authority]
        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            List<Links> list = LinksBll.GetAll().OrderBy(p => p.Status).ThenByDescending(l => l.Id).Skip((page - 1) * size).Take(size).ToList();
            var total = LinksBll.GetAll().Count();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 切换友情链接的白名单状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ToggleWhitelist(int id, bool state)
        {
            Links link = LinksBll.GetById(id);
            link.Except = !state;
            bool b = LinksBll.UpdateEntitySaved(link);
            return ResultData(null, b, b ? "切换成功！" : "切换失败！");
        }

        /// <summary>
        /// 切换友情链接可用状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Toggle(int id, bool state)
        {
            Links link = LinksBll.GetById(id);
            link.Status = !state ? Status.Available : Status.Unavailable;
            bool b = LinksBll.UpdateEntitySaved(link);
            return ResultData(null, b, b ? "切换成功！" : "切换失败！");
        }
    }
}