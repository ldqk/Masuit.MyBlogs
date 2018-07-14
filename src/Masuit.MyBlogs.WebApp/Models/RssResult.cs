using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;
using Common;
using Masuit.Tools.Html;
using Models.Entity;

namespace Masuit.MyBlogs.WebApp.Models
{
    /// <summary>
    /// 自定义RSS订阅
    /// </summary>
    public class RssResult : ActionResult
    {
        public List<Post> Posts { get; set; }

        public RssResult(List<Post> posts)
        {
            Posts = posts;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var items = new List<SyndicationItem>();
            foreach (var post in Posts)
            {
                string blogPostUrl = context.HttpContext.Request.Url?.Scheme + "://" + context.HttpContext.Request.Url?.Authority + "/" + post.Id;
                items.Add(new SyndicationItem(post.Title, post.Content.RemoveHtmlTag(200), new Uri(blogPostUrl), $"Blog:{post.Id}", post.ModifyDate));
            }
            context.HttpContext.Response.ContentType = "application/rss+xml";
            var feed = new SyndicationFeed(CommonHelper.GetSettings("Title"), "RSS Feed", context.HttpContext.Request.Url, items);
            var formatter = new Rss20FeedFormatter(feed);
            using (XmlWriter writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                formatter.WriteTo(writer);
            }
        }
    }
}