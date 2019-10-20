using System;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire
{
    public class IpIntercepter
    {
        public string IP { get; set; }
        public string RequestUrl { get; set; }
        public string Address { get; set; }
        public string UserAgent { get; set; }
        public DateTime Time { get; set; }
    }
}