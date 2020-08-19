using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 友情链接管理
    /// </summary>
    public class LinksController : BaseController
    {
        public IHttpClientFactory HttpClientFactory { get; set; }
        private HttpClient HttpClient => HttpClientFactory.CreateClient();

        /// <summary>
        /// 友情链接页
        /// </summary>
        /// <returns></returns>
        [Route("links"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Index()
        {
            var list = await LinksService.GetQueryFromCacheAsync<bool, LinksDto>(l => l.Status == Status.Available, l => l.Recommend, false);
            return CurrentUser.IsAdmin ? View("Index_Admin", list) : View(list);
        }

        /// <summary>
        /// 申请友链
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public async Task<ActionResult> Apply(Links links)
        {
            if (!links.Url.MatchUrl())
            {
                return ResultData(null, false, "添加失败！链接非法！");
            }

            var host = new Uri(links.Url).Host;
            if (LinksService.Any(l => l.Url.Contains(host)))
            {
                return ResultData(null, false, "添加失败！检测到您的网站已经是本站的友情链接了！");
            }

            HttpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
            HttpClient.DefaultRequestHeaders.Referrer = new Uri(Request.Scheme + "://" + Request.Host.ToString());
            return await (await HttpClient.GetAsync(links.Url).ContinueWith(async t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    return ResultData(null, false, "添加失败！检测到您的网站疑似挂了，或者连接到你网站的时候超时，请检查下！");
                }

                var res = await t;
                if (!res.IsSuccessStatusCode)
                {
                    return ResultData(null, false, "添加失败！检测到您的网站疑似挂了！返回状态码为：" + res.StatusCode);
                }

                var s = await res.Content.ReadAsStringAsync();
                if (!s.Contains(Request.Host.Host))
                {
                    return ResultData(null, false, $"添加失败！检测到您的网站上未将本站设置成友情链接，请先将本站主域名：{Request.Host}在您的网站设置为友情链接，并且能够展示后，再次尝试添加即可！");
                }

                var entry = await LinksService.GetAsync(l => l.Url.Equals(links.Url));
                bool b;
                if (entry is null)
                {
                    b = LinksService.AddEntitySaved(links) != null;
                }
                else
                {
                    entry.Url = links.Url;
                    entry.Except = links.Except;
                    entry.Name = links.Name;
                    entry.Recommend = links.Recommend;
                    b = await LinksService.SaveChangesAsync() > 0;
                }

                return ResultData(null, b, b ? "添加成功！这可能有一定的延迟，如果没有看到您的链接，请稍等几分钟后刷新页面即可，如有疑问，请联系站长。" : "添加失败！这可能是由于网站服务器内部发生了错误，如有疑问，请联系站长。");

            })).ConfigureAwait(false);
        }

        /// <summary>
        /// 添加友链
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Add(Links links)
        {
            var entry = await LinksService.GetByIdAsync(links.Id);
            bool b;
            if (entry is null)
            {
                b = await LinksService.AddEntitySavedAsync(links) > 0;
            }
            else
            {
                entry.Url = links.Url;
                entry.Except = links.Except;
                entry.Name = links.Name;
                entry.Recommend = links.Recommend;
                b = await LinksService.SaveChangesAsync() > 0;
            }

            return b ? ResultData(null, message: "添加成功！") : ResultData(null, false, "添加失败！");
        }

        /// <summary>
        /// 检测回链
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Check(string link)
        {
            HttpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
            return await (await HttpClient.GetAsync(link).ContinueWith(async t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    return ResultData(null, false, link + " 似乎挂了！");
                }

                var res = await t;
                if (!res.IsSuccessStatusCode)
                {
                    return ResultData(null, false, link + " 对方网站返回错误的状态码！http响应码为：" + res.StatusCode);
                }

                var s = await res.Content.ReadAsStringAsync();
                if (s.Contains(Request.Host.Host))
                {
                    return ResultData(null, true, "友情链接正常！");
                }

                return ResultData(null, false, link + " 对方似乎没有本站的友情链接！");
            })).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除友链
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Delete(int id)
        {
            bool b = await LinksService.DeleteByIdSavedAsync(id) > 0;
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 编辑友链
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Edit(Links model)
        {
            Links links = await LinksService.GetByIdAsync(model.Id);
            links.Name = model.Name;
            links.Url = model.Url;
            bool b = await LinksService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? "保存成功" : "保存失败");
        }

        /// <summary>
        /// 所有的友情链接
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult Get()
        {
            var list = LinksService.GetAll().OrderBy(p => p.Status).ThenByDescending(p => p.Recommend).ThenByDescending(p => p.Id).ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 切换友情链接的白名单状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        [MyAuthorize]
        public async Task<ActionResult> ToggleWhitelist(int id, bool state)
        {
            Links link = await LinksService.GetByIdAsync(id);
            link.Except = !state;
            bool b = await LinksService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? "切换成功！" : "切换失败！");
        }

        /// <summary>
        /// 切换友情链接的推荐状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        [MyAuthorize]
        public async Task<ActionResult> ToggleRecommend(int id, bool state)
        {
            Links link = await LinksService.GetByIdAsync(id);
            link.Recommend = !state;
            bool b = await LinksService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? "切换成功！" : "切换失败！");
        }

        /// <summary>
        /// 切换友情链接可用状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        [MyAuthorize]
        public async Task<ActionResult> Toggle(int id, bool state)
        {
            Links link = await LinksService.GetByIdAsync(id);
            link.Status = !state ? Status.Available : Status.Unavailable;
            bool b = await LinksService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? "切换成功！" : "切换失败！");
        }
    }
}