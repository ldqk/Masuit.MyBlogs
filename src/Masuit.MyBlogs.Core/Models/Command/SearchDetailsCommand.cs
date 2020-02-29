using System;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.Command
{
    /// <summary>
    /// 搜索详细记录输入模型
    /// </summary>
    public class SearchDetailsCommand
    {
        public SearchDetailsCommand()
        {
            SearchTime = DateTime.Now;
        }

        /// <summary>
        /// 关键词
        /// </summary>
        [Required(ErrorMessage = "关键词不能为空"), MaxLength(64, ErrorMessage = "关键词最大允许64个字符")]
        public string KeyWords { get; set; }

        /// <summary>
        /// 搜索时间
        /// </summary>
        public DateTime SearchTime { get; set; }

        /// <summary>
        /// 访问者IP
        /// </summary>
        public string IP { get; set; }
    }
}