using Masuit.MyBlogs.Core.Models.Entity;
using System.ComponentModel;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    /// <summary>
    ///
    /// </summary>
    public class AdvertisementViewModel : BaseEntity
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 宣传图片
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 小宣传图片
        /// </summary>
        public string ThumbImgUrl { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 广告url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 广告类型
        /// </summary>
        public string Types { get; set; }

        /// <summary>
        /// 访问次数
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// 曝光量
        /// </summary>
        public int DisplayCount { get; set; }

        /// <summary>
        /// 日均点击量
        /// </summary>
        public double AverageViewCount { get; set; }

        public string CategoryIds { get; set; }

        public string CategoryNames { get; set; }

        /// <summary>
        /// 到期时间
        /// </summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>
        /// 地区模式
        /// </summary>
        public RegionLimitMode RegionMode { get; set; }

        /// <summary>
        /// 地区，逗号或竖线分隔
        /// </summary>
        public string Regions { get; set; }
    }

    public class AdvertisementClickRecordViewModel
    {
        public string IP { get; set; }

        [Description("地理位置")]
        public string Location { get; set; }

        [Description("来源页面")]
        public string Referer { get; set; }

        [Description("访问时间")]
        public string Time { get; set; }
    }
}
