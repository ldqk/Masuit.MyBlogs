using System.ComponentModel;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    public class PostVisitRecordViewModel
    {
        public string IP { get; set; }

        [Description("地理位置")]
        public string Location { get; set; }

        [Description("来源页面")]
        public string Referer { get; set; }

        [Description("请求URL")]
        public string RequestUrl { get; set; }

        [Description("访问时间")]
        public string Time { get; set; }
    }
}
