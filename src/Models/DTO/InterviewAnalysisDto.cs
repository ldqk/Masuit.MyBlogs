using System;

namespace Models.DTO
{
    /// <summary>
    /// 访客分析模型
    /// </summary>
    public class InterviewAnalysisDto
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 浏览器版本
        /// </summary>
        public string BrowserType { get; set; }

        /// <summary>
        /// 来访时间
        /// </summary>
        public DateTime ViewTime { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 在线时长秒数
        /// </summary>
        public double OnlineSpanSeconds { get; set; }

    }
}