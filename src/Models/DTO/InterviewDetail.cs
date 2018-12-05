using System;

namespace Models.DTO
{
    /// <summary>
    /// 访客浏览详情
    /// </summary>
    public class InterviewDetail
    {
        /// <summary>
        /// 访问过的页面
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 访问时间
        /// </summary>
        public DateTime Time { get; set; }
    }
}