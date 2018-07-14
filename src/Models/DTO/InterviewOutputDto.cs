using System;

namespace Models.DTO
{
    /// <summary>
    /// 访客信息输出模型
    /// </summary>
    public class InterviewOutputDto
    {
        public long Id { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// UA
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// 浏览器版本
        /// </summary>
        public string BrowserType { get; set; }

        /// <summary>
        /// 来访时间
        /// </summary>
        public DateTime ViewTime { get; set; }

        /// <summary>
        /// 来自哪里
        /// </summary>
        public string FromUrl { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// ISP
        /// </summary>
        public string ISP { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// 详细地理位置
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 参考地理位置
        /// </summary>
        public string ReferenceAddress { get; set; }

        /// <summary>
        /// 着陆页
        /// </summary>
        public string LandPage { get; set; }

        /// <summary>
        /// 在线时长
        /// </summary>
        public string OnlineSpan { get; set; }
    }
}