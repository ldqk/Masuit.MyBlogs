using AutoMapper.QueryableExtensions;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.Tools.Html;
using NinjaNye.SearchExtensions;
using PanGu;
using PanGu.HighLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class PostService : BaseService<Post>, IPostService
    {
        public List<PostOutputDto> SearchPage<TOrder>(int page, int size, out int total, string[] keywords, Expression<Func<Post, TOrder>> orderBy)
        {
            var query = GetAllNoTracking().Search().Containing(keywords);
            total = query.Count();
            var posts = query.OrderByDescending(orderBy).Skip((page - 1) * size).Take(size).ProjectTo<PostOutputDto>().ToList();
            var simpleHtmlFormatter = new SimpleHTMLFormatter("<span style='color:red;background-color:yellow;font-size: 1.1em;font-weight:700;'>", "</span>");
            var highlighter = new Highlighter(simpleHtmlFormatter, new Segment()) { FragmentSize = 200 };

            foreach (var p in posts)
            {
                p.Content = p.Content.RemoveHtml();
                foreach (var s in keywords)
                {
                    string frag;
                    if (p.Title.Contains(s) && !string.IsNullOrEmpty(frag = highlighter.GetBestFragment(s, p.Title)))
                    {
                        p.Title = frag;
                        break;
                    }
                }
                bool handled = false;
                foreach (var s in keywords)
                {
                    string frag;
                    if (p.Content.Contains(s) && !string.IsNullOrEmpty(frag = highlighter.GetBestFragment(s, p.Content)))
                    {
                        p.Content = frag;
                        handled = true;
                        break;
                    }
                }
                if (p.Content.Length > 200 && !handled)
                {
                    p.Content = p.Content.Substring(0, 200);
                }
            }
            return posts;
        }

        public PostService(IBaseRepository<Post> repository) : base(repository)
        {
        }
    }
}