using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Common;
using IBLL;
using Models.DTO;
using Models.Entity;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class InterviewController : AdminController
    {
        public IInterviewBll InterviewBll { get; set; }

        public InterviewController(IInterviewBll interviewBll)
        {
            InterviewBll = interviewBll;
        }

        /// <summary>
        /// 分析访客数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Analysis(bool uniq = false)
        {
            var model = uniq ? AggregatedCounter.UniqueInterviews : AggregatedCounter.TotalInterviews;
            return ResultData(model);
        }

        public async Task<ActionResult> Get(int? days)
        {
            var time = DateTime.Now;
            if (days != null)
            {
                time = time.AddDays(-days.Value);
            }
            var list = await InterviewBll.LoadEntitiesNoTrackingAsync(i => i.ViewTime <= time, i => i.ViewTime, false);
            return ResultData(list);
        }

        public ActionResult GetPage(DateTime? start, DateTime? end, int page = 1, int size = 10, bool distinct = false, string search = "")
        {
            if (page <= 0)
            {
                page = 1;
            }
            if (start is null)
            {
                start = DateTime.Today;
            }
            if (end is null)
            {
                end = DateTime.Today.AddDays(1);
            }
            if (end > DateTime.Today.AddDays(1))
            {
                end = DateTime.Today.AddDays(1);
            }
            if (start > end)
            {
                start = end.Value.AddDays(-1);
            }
            int total;
            if (distinct)
            {
                var temp = string.IsNullOrEmpty(search) ? InterviewBll.SqlQuery<InterviewOutputDto>($"select Id,IP,UserAgent,OperatingSystem,BrowserType,ViewTime,FromUrl,Province,ISP,HttpMethod,Address,ReferenceAddress,LandPage,OnlineSpan from Interview where Id in(select max(Id) from Interview where ViewTime>='{start:yyyy-MM-dd}' and ViewTime<='{end:yyyy-MM-dd HH:mm:ss}' group by ip) ORDER BY ViewTime desc offset {(page - 1) * size} row fetch next {size} rows only") : InterviewBll.SqlQuery<InterviewOutputDto>($"select Id,IP,UserAgent,OperatingSystem,BrowserType,ViewTime,FromUrl,Province,ISP,HttpMethod,Address,ReferenceAddress,LandPage,OnlineSpan from Interview where Id in(select max(Id) from Interview  where ViewTime>='{start:yyyy-MM-dd}' and ViewTime<='{end:yyyy-MM-dd HH:mm:ss}' and (ip like '%{search}%' or Address like '%{search}%' or Province like '%{search}%' or ReferenceAddress like '%{search}%' or OperatingSystem like '%{search}%' or BrowserType like '%{search}%' or FromUrl like '%{search}%' or HttpMethod like '%{search}%' or ISP like '%{search}%' or UserAgent like '%{search}%') group by ip) ORDER BY ViewTime desc offset {(page - 1) * size} row fetch next {size} rows only");
                total = string.IsNullOrEmpty(search) ? InterviewBll.SqlQuery<int>($"SELECT count(1) from (SELECT DISTINCT ip from interview where ViewTime>='{start:yyyy-MM-dd}' and ViewTime<='{end:yyyy-MM-dd HH:mm:ss}') t").FirstOrDefault() : InterviewBll.SqlQuery<int>($"SELECT count(1) from (SELECT DISTINCT ip from interview where ViewTime>='{start:yyyy-MM-dd}' and ViewTime<='{end:yyyy-MM-dd HH:mm:ss}' and (ip like '%{search}%' or Address like '%{search}%' or Province like '%{search}%' or ReferenceAddress like '%{search}%' or OperatingSystem like '%{search}%' or BrowserType like '%{search}%' or FromUrl like '%{search}%' or HttpMethod like '%{search}%' or ISP like '%{search}%' or UserAgent like '%{search}%')) t").FirstOrDefault();
                var pages = Math.Ceiling(total * 1.0 / size).ToInt32();
                return PageResult(temp, pages, total);
            }
            var list = string.IsNullOrEmpty(search) ? InterviewBll.LoadPageEntitiesNoTracking<DateTime, InterviewOutputDto>(page, size, out total, i => i.ViewTime >= start && i.ViewTime <= end, i => i.ViewTime, false).ToList() : InterviewBll.LoadPageEntitiesNoTracking<DateTime, InterviewOutputDto>(page, size, out total, i => i.ViewTime >= start && i.ViewTime <= end && (i.IP.Contains(search) || i.Address.Contains(search) || i.Province.Contains(search) || i.ReferenceAddress.Contains(search) || i.OperatingSystem.Contains(search) || i.BrowserType.Contains(search) || i.FromUrl.Contains(search) || i.HttpMethod.Contains(search) || i.ISP.Contains(search) || i.UserAgent.Contains(search)), i => i.ViewTime, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        [AllowAnonymous]
        public ActionResult GetViewCount()
        {
            int count = InterviewBll.GetAll().Count();
            return ResultData(count);
        }

        [HttpPost]
        public ActionResult InterviewDetails(long id)
        {
            Interview interview = InterviewBll.GetById(id);
            return ResultData(new
            {
                interview = interview.Mapper<InterviewOutputDto>(),
                details = interview.InterviewDetails.Select(i => new
                {
                    i.Url,
                    i.Time
                })
            });
        }
    }
}