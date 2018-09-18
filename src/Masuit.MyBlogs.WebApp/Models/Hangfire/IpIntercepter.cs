using System;

namespace Masuit.MyBlogs.WebApp.Models.Hangfire
{
    public class IpIntercepter
    {
        public string IP { get; set; }
        public string RequestUrl { get; set; }
        public DateTime Time { get; set; }
        public string Address { get; set; }
    }
}