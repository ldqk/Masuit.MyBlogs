using System;

namespace Models.ViewModel
{
    /// <summary>
    /// 分页数据模型
    /// </summary>
    public class PageDataModel
    {
        /// <summary>
        /// 分页数据模型
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="pageCount">页数</param>
        /// <param name="totalCount">总条数</param>
        public PageDataModel(object data, int pageCount, int totalCount)
        {
            this.Data = data;
            this.PageCount = pageCount;
            this.TotalCount = totalCount;
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        public Object Data { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; set; }
    }
}