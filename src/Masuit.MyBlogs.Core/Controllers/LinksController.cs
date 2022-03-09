using EFCoreSecondLevelCacheInterceptor;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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
        public async Task<ActionResult> Index([FromServices] IWebHostEnvironment hostEnvironment)
        {
            var list = await LinksService.GetQueryFromCacheAsync<bool, LinksDto>(l => l.Status == Status.Available, l => l.Recommend, false);
            ViewBag.Html = await new FileInfo(Path.Combine(hostEnvironment.WebRootPath, "template", "links.html")).ShareReadWrite().ReadAllTextAsync(Encoding.UTF8);
            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage, Request.Location());
            return CurrentUser.IsAdmin ? View("Index_Admin", list) : View(list);
        }

        /// <summary>
        /// 申请友链
        /// </summary>
        /// <param name="link"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ActionResult> Apply(Links link, CancellationToken cancellationToken)
        {
            if (!link.Url.MatchUrl() || link.Url.Contains(Request.Host.Host))
            {
                return ResultData(null, false, "添加失败！链接非法！");
            }

            if (link.Url.Contains(new[] { "?", "&", "=" }))
            {
                return ResultData(null, false, "添加失败！请移除链接中的查询字符串后再试！如遇特殊情况，请联系站长进行处理。");
            }

            if (!link.Url.Contains(link.UrlBase))
            {
                return ResultData(null, false, "站点主页和友链地址不匹配，请检查");
            }

            var host = new Uri(link.Url).Host;
            if (LinksService.Any(l => l.Url.Contains(host)))
            {
                return ResultData(null, false, "添加失败！检测到您的网站已经是本站的友情链接了！");
            }

            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36 Edg/93.0.961.47");
            HttpClient.DefaultRequestHeaders.Referrer = new Uri(Request.Scheme + "://" + Request.Host);
            HttpClient.DefaultRequestHeaders.Add("X-Forwarded-For", "1.1.1.1");
            HttpClient.DefaultRequestHeaders.Add("X-Forwarded-Host", "1.1.1.1");
            HttpClient.DefaultRequestHeaders.Add("X-Real-IP", "1.1.1.1");
            HttpClient.DefaultRequestVersion = new Version(2, 0);
            return await HttpClient.GetAsync(link.Url, cancellationToken).ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    return ResultData(null, false, "添加失败！检测到您的网站疑似挂了，或者连接到你网站的时候超时，请检查下！");
                }

                var res = t.Result;
                if (!res.IsSuccessStatusCode)
                {
                    return ResultData(null, false, "添加失败！检测到您的网站疑似挂了！返回状态码为：" + res.StatusCode);
                }

                using var httpContent = res.Content;
                var s = httpContent.ReadAsStringAsync().Result;
                if (!s.Contains(Request.Host.Host))
                {
                    return ResultData(null, false, $"添加失败！检测到您的网站上未将本站设置成友情链接，请先将本站主域名：{Request.Host}在您的网站设置为友情链接，并且能够展示后，再次尝试添加即可！");
                }

                var b = LinksService.AddEntitySaved(link) != null;
                return ResultData(null, b, b ? "添加成功！这可能有一定的延迟，如果没有看到您的链接，请稍等几分钟后刷新页面即可，如有疑问，请联系站长。" : "添加失败！这可能是由于网站服务器内部发生了错误，如有疑问，请联系站长。");
            });
        }

        /// <summary>
        /// 添加友链
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Add(Links links)
        {
            bool b = await LinksService.AddOrUpdateSavedAsync(l => l.Id, links) > 0;
            return b ? ResultData(null, message: "添加成功！") : ResultData(null, false, "添加失败！");
        }

        /// <summary>
        /// 检测回链
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        [MyAuthorize]
        public Task<ActionResult> Check(string link)
        {
            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36 Edg/93.0.961.47");
            HttpClient.DefaultRequestHeaders.Add("X-Forwarded-For", "1.1.1.1");
            HttpClient.DefaultRequestHeaders.Add("X-Forwarded-Host", "1.1.1.1");
            HttpClient.DefaultRequestHeaders.Add("X-Real-IP", "1.1.1.1");
            HttpClient.DefaultRequestVersion = new Version(2, 0);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            return HttpClient.GetAsync(link, cts.Token).ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    return ResultData(null, false, link + " 似乎挂了！错误信息：" + t.Exception?.Flatten().InnerException?.Message);
                }

                using var res = t.Result;
                if (!res.IsSuccessStatusCode)
                {
                    return ResultData(null, false, link + " 对方网站返回错误的状态码！http响应码为：" + res.StatusCode);
                }

                using var httpContent = res.Content;
                var s = httpContent.ReadAsStringAsync().Result;
                return s.Contains(CommonHelper.SystemSettings["Domain"].Split("|")) ? ResultData(null, true, "友情链接正常！") : ResultData(null, false, link + " 对方似乎没有本站的友情链接！");
            });
        }

        /// <summary>
        /// 删除友链
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Delete(int id)
        {
            bool b = await LinksService.DeleteByIdAsync(id) > 0;
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
            var b = await LinksService.GetQuery(m => m.Id == model.Id).UpdateFromQueryAsync(m => new Links()
            {
                Name = model.Name,
                Url = model.Url,
                UrlBase = model.UrlBase
            }) > 0;
            return ResultData(null, b, b ? "保存成功" : "保存失败");
        }

        /// <summary>
        /// 所有的友情链接
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult Get()
        {
            var list = LinksService.GetAll<LinksDto>().OrderBy(p => p.Status).ThenByDescending(p => p.Recommend).ThenByDescending(p => p.Id).NotCacheable().ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 切换友情链接的白名单状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [MyAuthorize]
        public async Task<ActionResult> ToggleWhitelist(int id)
        {
            var b = await LinksService.GetQuery(m => m.Id == id).UpdateFromQueryAsync(m => new Links()
            {
                Except = !m.Except
            }) > 0;
            return ResultData(null, b, b ? "切换成功！" : "切换失败！");
        }

        /// <summary>
        /// 切换友情链接的推荐状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [MyAuthorize]
        public async Task<ActionResult> ToggleRecommend(int id)
        {
            var b = await LinksService.GetQuery(m => m.Id == id).UpdateFromQueryAsync(m => new Links()
            {
                Recommend = !m.Recommend
            }) > 0;
            return ResultData(null, b, b ? "切换成功！" : "切换失败！");
        }

        /// <summary>
        /// 切换友情链接可用状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [MyAuthorize]
        public async Task<ActionResult> Toggle(int id)
        {
            var b = await LinksService.GetQuery(m => m.Id == id).UpdateFromQueryAsync(m => new Links()
            {
                Status = m.Status == Status.Unavailable ? Status.Available : Status.Unavailable
            }) > 0;
            return ResultData(null, b, b ? "切换成功！" : "切换失败！");
        }
    }
}
