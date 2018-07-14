using System.Collections.Generic;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.WebApp.Models
{
    public class BaiduAnalysisModel
    {

        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    public class Result
    {
        [JsonProperty("res")]
        public Res Res { get; set; }
    }

    public class Res
    {
        [JsonProperty("keyword_list")]
        public List<string> KeywordList { get; set; }

        [JsonProperty("real_title")]
        public string RealTitle { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("wordpos")]
        public List<string> Wordpos { get; set; }

        [JsonProperty("wordrank")]
        public List<string> Wordrank { get; set; }
    }

}