using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entity
{
    /// <summary>
    /// 访客浏览详情
    /// </summary>
    [Table("InterviewDetail")]
    public class InterviewDetail
    {
        public InterviewDetail()
        {
            Time = DateTime.Now;
        }

        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 访问过的页面
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 访问时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 关联的访客表ID
        /// </summary>
        [ForeignKey("Interview")]
        public long InterviewId { get; set; }

        public virtual Interview Interview { get; set; }
    }
}