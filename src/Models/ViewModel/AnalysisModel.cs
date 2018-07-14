using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models.ViewModel
{
    /// <summary>
    /// 访客统计分析模型
    /// </summary>
    public class AnalysisModel
    {
        /// <summary>
        /// 当日唯一访客数
        /// </summary>
        [JsonProperty("todayuv")]
        public int Todayuv { get; set; }

        /// <summary>
        /// 当日页面访客数
        /// </summary>
        [JsonProperty("todaypv")]
        public int Todaypv { get; set; }

        /// <summary>
        /// 当月唯一访客数
        /// </summary>
        [JsonProperty("monthuv")]
        public int Monthuv { get; set; }

        /// <summary>
        /// 当月页面访客数
        /// </summary>
        [JsonProperty("monthpv")]
        public int Monthpv { get; set; }

        /// <summary>
        /// 今年唯一访客数
        /// </summary>
        [JsonProperty("yearpv")]
        public int Yearpv { get; set; }

        /// <summary>
        /// 今年页面访客数
        /// </summary>
        [JsonProperty("yearuv")]
        public int Yearuv { get; set; }

        /// <summary>
        /// 总页面访客数
        /// </summary>
        [JsonProperty("totalpv")]
        public int Totalpv { get; set; }

        /// <summary>
        /// 总唯一访客数
        /// </summary>
        [JsonProperty("totaluv")]
        public int Totaluv { get; set; }

        /// <summary>
        /// 最高面访客数
        /// </summary>
        [JsonProperty("highpv")]
        public object Highpv { get; set; }

        /// <summary>
        /// 最高唯一访客数
        /// </summary>
        [JsonProperty("highuv")]
        public object Highuv { get; set; }

        /// <summary>
        /// 访客浏览器类型集
        /// </summary>
        [JsonProperty("client")]
        public List<object> Client { get; set; }

        /// <summary>
        /// 访客地区集
        /// </summary>
        [JsonProperty("region")]
        public List<object> Region { get; set; }

        /// <summary>
        /// 访客浏览器集
        /// </summary>
        [JsonProperty("browser")]
        public List<object[]> Browser { get; set; }

        /// <summary>
        /// PV集
        /// </summary>
        [JsonProperty("pv")]
        public List<List<object>> Pv { get; set; }

        /// <summary>
        /// UV集
        /// </summary>
        [JsonProperty("uv")]
        public List<List<object>> Uv { get; set; }

        /// <summary>
        /// 每日新增独立访客集
        /// </summary>
        [JsonProperty("iv")]
        public List<List<object>> Iv { get; set; }

        /// <summary>
        /// 跳出率
        /// </summary>
        public string BounceRate { get; set; }

        /// <summary>
        /// 当日跳出率
        /// </summary>
        public string BounceRateToday { get; set; }

        /// <summary>
        /// 跳出率聚合统计结果
        /// </summary>
        public object BounceRateAggregate { get; set; }

        /// <summary>
        /// 访问时长统计
        /// </summary>
        public object OnlineSpanAggregate { get; set; }
    }

    /// <summary>
    /// 跳出率
    /// </summary>
    public class BounceRate
    {
        /// <summary>
        /// 跳出人数
        /// </summary>
        public double Dap { get; set; }

        /// <summary>
        /// 所有访客人数
        /// </summary>
        public int All { get; set; }

        /// <summary>
        /// 跳出率
        /// </summary>
        public double Result { get; set; }
    }

    public class BounceRateAggregate
    {
        /// <summary>
        /// 跳出率统计
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 跳出人数
        /// </summary>
        public int Dap { get; set; }

        /// <summary>
        /// 总访问人数
        /// </summary>
        public int All { get; set; }

        /// <summary>
        /// 跳出率
        /// </summary>
        public decimal Rate { get; set; }
    }
}