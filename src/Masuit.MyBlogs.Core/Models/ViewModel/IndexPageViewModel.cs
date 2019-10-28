using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    /// <summary>
    /// 首页视图模型
    /// </summary>
    public class IndexPageViewModel
    {
        /// <summary>
        /// 文章列表
        /// </summary>
        public IList<PostOutputDto> Posts { get; set; }

        /// <summary>
        /// 网站公告列表
        /// </summary>
        public IList<NoticeOutputDto> Notices { get; set; }

        /// <summary>
        /// 分类列表
        /// </summary>
        public IList<CategoryOutputDto> Categories { get; set; }

        /// <summary>
        /// 标签列表
        /// </summary>
        public IDictionary<string, int> Tags { get; set; }

        /// <summary>
        /// 近期热搜
        /// </summary>
        public List<KeywordsRankOutputDto> HotSearch { get; set; }

        /// <summary>
        /// 热门文章
        /// </summary>
        public List<PostOutputDto> Top6Post { get; set; }

        /// <summary>
        /// 文章列表查询
        /// </summary>
        public IQueryable<PostOutputDto> PostsQueryable { get; set; }

        /// <summary>
        /// banner文章
        /// </summary>
        public List<Advertisement> Banner { get; set; }

        /// <summary>
        /// 边栏广告
        /// </summary>
        public List<Advertisement> SidebarAds { get; set; }

        /// <summary>
        /// 列表内广告
        /// </summary>
        public Advertisement ListAdvertisement { get; set; }
    }
}