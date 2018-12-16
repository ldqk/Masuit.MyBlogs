using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entity
{
    [Table("InterviewAggregate")]
    public class InterviewAggregate : BaseEntity
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 直接访问量
        /// </summary>
        public int Pv { get; set; }

        /// <summary>
        /// 独立访客
        /// </summary>
        public int Uv { get; set; }

        /// <summary>
        /// 新增的独立访客
        /// </summary>
        public int Iv { get; set; }

        /// <summary>
        /// 跳出人数
        /// </summary>
        public int Dap { get; set; }

        /// <summary>
        /// 访问人数
        /// </summary>
        public int ViewAll { get; set; }

        /// <summary>
        /// 跳出率
        /// </summary>
        public decimal BounceRate { get; set; }

    }
}