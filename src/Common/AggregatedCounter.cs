using Models.ViewModel;

namespace Common
{
    /// <summary>
    /// 访客统计缓存
    /// </summary>
    public class AggregatedCounter
    {
        /// <summary>
        /// 独立访客统计汇总
        /// </summary>
        public static AnalysisModel UniqueInterviews { get; set; } = new AnalysisModel();

        /// <summary>
        /// 直接访客统计汇总
        /// </summary>
        public static AnalysisModel TotalInterviews { get; set; } = new AnalysisModel();
    }
}