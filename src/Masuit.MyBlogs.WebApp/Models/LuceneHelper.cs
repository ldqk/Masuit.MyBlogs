using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.WebApp.Models
{
    /// <summary>
    /// Lucene帮助类
    /// </summary>
    public static class LuceneHelper
    {
        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static List<string> CutKeywords(string keyword)
        {
            var list = new HashSet<string>
            {
                keyword
            };
            var mc = Regex.Matches(keyword, @"(([A-Z]*[a-z]*)[\d]*)([\u4E00-\u9FA5]+)*((?!\p{P}).)*");
            foreach (Match m in mc)
            {
                list.Add(m.Value);
                foreach (Group g in m.Groups)
                {
                    list.Add(g.Value);
                }
            }
            if (keyword.Length >= 6)
            {
                using (HttpClient client = new HttpClient()
                {
                    BaseAddress = new Uri("http://zhannei.baidu.com")
                })
                {
                    try
                    {
                        var res = client.GetAsync($"/api/customsearch/keywords?title={keyword}").Result;
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            BaiduAnalysisModel model = JsonConvert.DeserializeObject<BaiduAnalysisModel>(res.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
                            if (model.Result.Res.KeywordList != null && model.Result.Res.KeywordList.Any())
                            {
                                list.AddRange(model.Result.Res.KeywordList.ToArray());
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            list.RemoveWhere(s => s.Length < 2 || Regex.IsMatch(s, @"^\p{P}.*"));
            return list.OrderByDescending(s => s.Length).ToList();
        }
    }
}