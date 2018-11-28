using Common;
using Masuit.Tools;
using Masuit.Tools.NoSQL;
using Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class InterviewController : AdminController
    {
        //public IInterviewBll InterviewBll { get; set; }
        public RedisHelper RedisHelper { get; set; }
        public InterviewController(RedisHelper redisHelper)
        {
            RedisHelper = redisHelper;
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

        public ActionResult Get(int? days)
        {
            var time = DateTime.Now;
            if (days != null)
            {
                time = time.AddDays(-days.Value);
            }
            var list = new List<Interview>();
            for (var i = time; i < DateTime.Now; i = i.AddDays(1))
            {
                list.AddRange(RedisHelper.ListRange<Interview>($"Interview:{i:yyyy:MM:dd}").Where(x => x.ViewTime >= time).OrderByDescending(x => x.ViewTime));
            }
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
                List<Interview> temp = new List<Interview>();
                for (var i = start.Value; i < end; i = i.AddDays(1))
                {
                    var @where = string.IsNullOrEmpty(search) ? (Func<Interview, bool>)(x => x.ViewTime > start) : x => x.ViewTime > start && (x.IP.Contains(search) || x.Address.Contains(search) || x.Province.Contains(search) || x.ReferenceAddress.Contains(search) || x.OperatingSystem.Contains(search) || x.FromUrl.Contains(search) || x.UserAgent.Contains(search));
                    var query = RedisHelper.ListRange<Interview>($"Interview:{i:yyyy:MM:dd}").Where(where).DistinctBy(x => x.IP);
                    temp.AddRange(query);
                }
                total = temp.Count;
                var pages = Math.Ceiling(total * 1.0 / size).ToInt32();
                return PageResult(temp.OrderByDescending(i => i.ViewTime).Skip((page - 1) * size).Take(size), pages, total);
            }
            List<Interview> list = new List<Interview>();
            for (var i = start.Value; i < end; i = i.AddDays(1))
            {
                var @where = string.IsNullOrEmpty(search) ? (Func<Interview, bool>)(x => x.ViewTime > start) : x => x.ViewTime > start && (x.IP.Contains(search) || x.Address.Contains(search) || x.Province.Contains(search) || x.ReferenceAddress.Contains(search) || x.OperatingSystem.Contains(search) || x.FromUrl.Contains(search) || x.UserAgent.Contains(search));
                var query = RedisHelper.ListRange<Interview>($"Interview:{i:yyyy:MM:dd}").Where(where);
                list.AddRange(query);
            }
            total = list.Count;
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list.Skip((page - 1) * size).Take(size), pageCount, total);
        }

        [AllowAnonymous]
        public ActionResult GetViewCount()
        {
            var count = RedisHelper.GetString<double>("Interview:ViewCount");
            return ResultData(count);
        }

        //[HttpPost]
        //public ActionResult InterviewDetails(long id)
        //{
        //    Interview interview = InterviewBll.GetById(id);
        //    return ResultData(new
        //    {
        //        interview = interview.Mapper<InterviewOutputDto>(),
        //        details = interview.InterviewDetails.Select(i => new
        //        {
        //            i.Url,
        //            i.Time
        //        })
        //    });
        //}
    }
}