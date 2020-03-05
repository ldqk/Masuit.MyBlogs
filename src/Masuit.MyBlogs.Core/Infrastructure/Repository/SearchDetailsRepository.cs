using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class SearchDetailsRepository : BaseRepository<SearchDetails>, ISearchDetailsRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override SearchDetails AddEntity(SearchDetails t)
        {
            DataContext.Add(t);
            return t;
        }

        public List<SearchRank> GetRanks(DateTime start)
        {
            return DataContext.SearchRanks.FromSqlRaw($"SELECT KeyWords Keywords,COUNT(*) Count from (SELECT IP,KeyWords FROM `searchdetails` WHERE SearchTime>'{start:yyyy-MM-dd}' GROUP BY IP,KeyWords) as t GROUP BY KeyWords ORDER BY COUNT(1) DESC LIMIT 30").ToList();
        }
    }
}