using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    public class AdvertisementDto : BaseDto
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "标题不能为空"), MinLength(10, ErrorMessage = "标题建议设置为10-128字"), MaxLength(128, ErrorMessage = "标题建议设置为10-128字")]
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
        [Required(ErrorMessage = "描述文字不能为空"), MinLength(50, ErrorMessage = "标题建议设置为50-1000字"), MaxLength(1000, ErrorMessage = "标题建议设置为50-1000字")]
        public string Description { get; set; }

        /// <summary>
        /// 广告url
        /// </summary>
        [Required(ErrorMessage = "推广链接不能为空")]
        public string Url { get; set; }

        /// <summary>
        /// 权重
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 广告类型
        /// </summary>
        [Required(ErrorMessage = "类型不能为空，至少需要选择一个类型")]
        public string Types { get; set; }

        public string CategoryId { get; set; }
    }
}