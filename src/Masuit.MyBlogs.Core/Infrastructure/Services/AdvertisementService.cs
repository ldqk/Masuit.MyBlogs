using CacheManager.Core;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class AdvertisementService : BaseService<Advertisement>, IAdvertisementService
    {
        public ICacheManager<List<Advertisement>> CacheManager { get; set; }

        public AdvertisementService(IBaseRepository<Advertisement> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }

        /// <summary>
        /// 按价格随机筛选一个元素
        /// </summary>
        /// <param name="type">广告类型</param>
        /// <param name="cid">分类id</param>
        /// <returns></returns>
        public Advertisement GetByWeightedPrice(AdvertiseType type, string location, int? cid = null)
        {
            return GetsByWeightedPrice(1, type, location, cid).FirstOrDefault();
        }

        /// <summary>
        /// 按价格随机筛选一个元素
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="type">广告类型</param>
        /// <param name="cid">分类id</param>
        /// <returns></returns>
        public List<Advertisement> GetsByWeightedPrice(int count, AdvertiseType type, string location, int? cid = null)
        {
            return CacheManager.GetOrAdd($"Advertisement:{location.Crc32()}:{type}:{count}-{cid}", _ =>
            {
                var atype = type.ToString("D");
                Expression<Func<Advertisement, bool>> where = a => a.Types.Contains(atype) && a.Status == Status.Available;
                where = where.And(a => a.RegionMode == RegionLimitMode.All || (a.RegionMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, a.Regions) : !Regex.IsMatch(location, a.Regions)));
                if (cid.HasValue)
                {
                    var scid = cid.ToString();
                    if (Any(a => a.CategoryIds.Contains(scid)))
                    {
                        where = where.And(a => a.CategoryIds.Contains(scid) || string.IsNullOrEmpty(a.CategoryIds));
                    }
                }

                var list = GetQuery(where).OrderBy(a => -Math.Log(DataContext.Random()) / ((double)a.Price / a.Types.Length) * (string.IsNullOrEmpty(a.CategoryIds) ? 5 : 1)).Take(count).ToList();
                var ids = list.Select(a => a.Id).ToArray();
                GetQuery(a => ids.Contains(a.Id)).UpdateFromQuery(a => new Advertisement()
                {
                    DisplayCount = a.DisplayCount + 1
                });

                return list;
            });
        }
    }
}