using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 搜索详细记录
    /// </summary>
    [Table("SearchDetails")]
    public class SearchDetails
    {
        public SearchDetails()
        {
            SearchTime = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        [Required(ErrorMessage = "关键词不能为空")]
        public string Keywords { get; set; }

        /// <summary>
        /// 搜索时间
        /// </summary>
        public DateTime SearchTime { get; set; }

        /// <summary>
        /// 访问者IP
        /// </summary>
        public string IP { get; set; }
    }

    public class SearchRank
    {
        public string Keywords { get; set; }
        public int Count { get; set; }
    }
}