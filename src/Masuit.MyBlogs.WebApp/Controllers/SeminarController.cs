using Common;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.Net;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    [RoutePrefix("c")]
    public class SeminarController : BaseController
    {
        public ISeminarBll SeminarBll { get; set; }
        public IPostBll PostBll { get; set; }

        public SeminarController(ISeminarBll seminarBll, IPostBll postBll)
        {
            SeminarBll = seminarBll;
            PostBll = postBll;
        }

        [Route("{id:int}/{page:int?}/{size:int?}")]
        public ActionResult Index(int id, int page = 1, int size = 15, OrderBy orderBy = OrderBy.ModifyDate)
        {
            IList<Post> posts;
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var s = SeminarBll.GetById(id);
            if (s is null)
            {
                return RedirectToAction("Index", "Error");
            }
            var temp = PostBll.LoadEntities(p => p.Seminar.Any(x => x.Id == id) && (p.Status == Status.Pended || user.IsAdmin)).OrderByDescending(p => p.IsFixedTop);
            switch (orderBy)
            {
                case OrderBy.CommentCount:
                    posts = temp.ThenByDescending(p => p.Comment.Count).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.PostDate:
                    posts = temp.ThenByDescending(p => p.PostDate).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.ViewCount:
                    posts = temp.ThenByDescending(p => p.PostAccessRecord.Sum(r => r.ClickCount)).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.VoteCount:
                    posts = temp.ThenByDescending(p => p.VoteUpCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                default:
                    posts = temp.ThenByDescending(p => p.ModifyDate).Skip(size * (page - 1)).Take(size).ToList();
                    break;
            }
            ViewBag.Total = temp.Count();
            ViewBag.Title = s.Title;
            ViewBag.Desc = s.Description;
            ViewBag.SubTitle = s.SubTitle;
            return View(posts.Mapper<IList<PostOutputDto>>());
        }

        #region 管理端

        [Authority]
        public ActionResult Save(Seminar seminar)
        {
            bool contain;
            if (seminar.Id > 0)
            {
                //更新
                contain = SeminarBll.GetAll().Select(s => s.Title).Except(new List<string>()
                {
                    SeminarBll.GetById(seminar.Id).Title
                }).Contains(seminar.Title);
            }
            else
            {
                //添加
                contain = SeminarBll.GetAll().Select(s => s.Title).Contains(seminar.Title);
            }
            if (contain)
            {
                return ResultData(null, false, $"{seminar.Title} 已经存在了");
            }
            var b = SeminarBll.AddOrUpdateSaved(s => s.Id, seminar) > 0;
            return ResultData(null, b, b ? "保存成功" : "保存失败");
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            bool b = SeminarBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        [Authority]
        public ActionResult Get(int id)
        {
            Seminar seminar = SeminarBll.GetById(id);
            return ResultData(seminar.Mapper<SeminarOutputDto>());
        }

        [Authority]
        public ActionResult GetPageData(int page, int size)
        {
            List<SeminarOutputDto> list = SeminarBll.LoadPageEntitiesNoTracking<int, SeminarOutputDto>(page, size, out int total, s => true, s => s.Id, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        [Authority]
        public ActionResult GetAll()
        {
            List<SeminarOutputDto> list = SeminarBll.GetAll<string, SeminarOutputDto>(s => s.Title).ToList();
            return ResultData(list);
        }

        [Authority]
        public ActionResult AddPost(int id, int pid)
        {
            Seminar seminar = SeminarBll.GetById(id);
            Post post = PostBll.GetById(pid);
            seminar.Post.Add(post);
            bool b = SeminarBll.UpdateEntitySaved(seminar);
            return ResultData(null, b, b ? $"已成功将【{post.Title}】添加到专题【{seminar.Title}】" : "添加失败！");
        }

        [Authority]
        public ActionResult RemovePost(int id, int pid)
        {
            Seminar seminar = SeminarBll.GetById(id);
            Post post = PostBll.GetById(pid);
            seminar.Post.Remove(post);
            bool b = SeminarBll.UpdateEntitySaved(seminar);
            return ResultData(null, b, b ? $"已成功将【{post.Title}】从专题【{seminar.Title}】移除" : "添加失败！");
        }

        #endregion
    }
}