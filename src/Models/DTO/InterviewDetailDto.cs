using System;

namespace Models.DTO
{
    public class InterviewDetailDto
    {
        public InterviewDetailDto()
        {
            Time = DateTime.Now;
        }

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