using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using System.Text.RegularExpressions;
using FreeRedis;
using Masuit.MyBlogs.Core.Models;
using Masuit.Tools.Core;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed class AdvertisementService(IBaseRepository<Advertisement> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Advertisement>(repository, searchEngine, searcher), IAdvertisementService
{
    public IRedisClient CacheManager { get; set; }

    public ICategoryRepository CategoryRepository { get; set; }

    /// <summary>
    /// 按价格随机筛选一个元素
    /// </summary>
    /// <param name="type">广告类型</param>
    /// <param name="location"></param>
    /// <param name="cid">分类id</param>
    /// <param name="keywords"></param>
    /// <returns></returns>
    public AdvertisementDto GetByWeightedPrice(AdvertiseType type, IPLocation location, int? cid = null, string keywords = "")
    {
        return GetsByWeightedPrice(1, type, location, cid, keywords).FirstOrDefault();
    }

    /// <summary>
    /// 按价格随机筛选一个元素
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="type">广告类型</param>
    /// <param name="location"></param>
    /// <param name="cid">分类id</param>
    /// <param name="keywords"></param>
    /// <returns></returns>
    public List<AdvertisementDto> GetsByWeightedPrice(int count, AdvertiseType type, IPLocation location, int? cid = null, string keywords = "")
    {
        if (Count(a => a.Status == Status.Available) >= 200)
        {
            return GetsByWeightedPriceExternal(count, type, location, cid, keywords);
        }
        return GetsByWeightedPriceMemory(count, type, location, cid, keywords);
    }

    /// <summary>
    /// 按价格随机筛选一个元素
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="type">广告类型</param>
    /// <param name="ipinfo"></param>
    /// <param name="cid">分类id</param>
    /// <param name="keywords"></param>
    /// <returns></returns>
    public List<AdvertisementDto> GetsByWeightedPriceExternal(int count, AdvertiseType type, IPLocation ipinfo, int? cid = null, string keywords = "")
    {
        var (location, _, _) = ipinfo;
        return CacheManager.GetOrAdd($"Advertisement:{location}:{type}:{count}-{cid}-{keywords}", () =>
        {
            var atype = type.ToString("D");
            Expression<Func<Advertisement, bool>> where = a => a.Types.Contains(atype) && a.Status == Status.Available;
            var categories = CacheManager.GetOrAdd("Category:all", () => CategoryRepository.GetAll().Select(c => new { c.Id, c.Path }).Distinct().ToArray(), TimeSpan.FromHours(5));
            var catCount = categories.Length;
            where = where.And(a => a.RegionMode == RegionLimitMode.All || (a.RegionMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase) : !Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase)));
            if (cid.HasValue)
            {
                var scid = categories.FirstOrDefault(c => c.Id == cid.Value)!.Path.Replace(',', '|');
                if (Any(a => Regex.IsMatch(a.CategoryIds, scid)))
                {
                    where = where.And(a => Regex.IsMatch(a.CategoryIds, scid) || string.IsNullOrEmpty(a.CategoryIds));
                }
            }

            if (!keywords.IsNullOrEmpty())
            {
                var regex = Searcher.CutKeywords(keywords).Select(Regex.Escape).Join("|");
                where = where.And(a => Regex.IsMatch(a.Title + a.Description, regex));
            }

            var array = GetQuery(a => a.Types.Contains(atype) && a.Status == Status.Available).GroupBy(a => a.Merchant).Select(g => g.OrderBy(_ => EF.Functions.Random()).FirstOrDefault().Id).Take(50).ToArray();
            var list = GetQuery(where).Where(a => array.Contains(a.Id)).OrderBy(a => -Math.Log(EF.Functions.Random()) / ((double)a.Price / a.Types.Length * catCount / (string.IsNullOrEmpty(a.CategoryIds) ? catCount : (a.CategoryIds.Length + 1)))).Take(count).ProjectDto().ToListWithNoLock();
            if (list.Count == 0 && keywords is { Length: > 0 })
            {
                return GetsByWeightedPriceExternal(count, type, ipinfo, cid);
            }

            var ids = list.Select(a => a.Id).ToArray();
            GetQuery(a => ids.Contains(a.Id)).ExecuteUpdate(p => p.SetProperty(a => a.DisplayCount, a => a.DisplayCount + 1));
            return list;
        }, TimeSpan.FromMinutes(10));
    }

    /// <summary>
    /// 按价格随机筛选一个元素
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="type">广告类型</param>
    /// <param name="ipinfo"></param>
    /// <param name="cid">分类id</param>
    /// <param name="keywords"></param>
    /// <returns></returns>
    public List<AdvertisementDto> GetsByWeightedPriceMemory(int count, AdvertiseType type, IPLocation ipinfo, int? cid = null, string keywords = "")
    {
        var (location, _, _) = ipinfo;
        return CacheManager.GetOrAdd($"Advertisement:{location}:{type}:{count}-{cid}-{keywords}", () =>
        {
            var all = CacheManager.GetOrAdd("Advertisement:all", () => GetQuery(a => a.Status == Status.Available).ProjectDto().ToListWithNoLock(), TimeSpan.FromMinutes(10));
            var atype = type.ToString("D");
            var categories = CacheManager.GetOrAdd("Category:all", () => CategoryRepository.GetAll().Select(c => new { c.Id, c.Path }).Distinct().ToArray(), TimeSpan.FromHours(5)).ToDictionarySafety(a => a.Id, a => a.Path.Replace(',', '|'));
            var catCount = categories.Count;
            string scid = "";
            if (cid.HasValue)
            {
                scid = categories[cid.Value];
            }

            var array = all.Where(a => a.Types.Contains(atype)).GroupBy(a => a.Merchant).Select(static g => g.OrderByRandom().FirstOrDefault().Id).Take(50).ToArray();
            var list = all.Where(a => a.Types.Contains(atype) && array.Contains(a.Id))
                .Where(a => a.RegionMode == RegionLimitMode.All || (a.RegionMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase) : !Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase)))
                .WhereIf(cid.HasValue, a => Regex.IsMatch(a.CategoryIds + "", scid) || string.IsNullOrEmpty(a.CategoryIds))
                .WhereIf(!keywords.IsNullOrEmpty(), a => (a.Title + a.Description).Contains(Searcher.CutKeywords(keywords)))
                .OrderBy(a => -Math.Log(Random.Shared.NextDouble()) / ((double)a.Price / a.Types.Length * catCount / (string.IsNullOrEmpty(a.CategoryIds) ? catCount : (a.CategoryIds.Length + 1))))
                .Take(count).ToList();
            if (list.Count == 0 && keywords is { Length: > 0 })
            {
                list.AddRange(all.Where(a => a.Types.Contains(atype) && array.Contains(a.Id))
                .Where(a => a.RegionMode == RegionLimitMode.All || (a.RegionMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase) : !Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase)))
                .WhereIf(cid.HasValue, a => Regex.IsMatch(a.CategoryIds + "", scid) || string.IsNullOrEmpty(a.CategoryIds))
                .OrderBy(a => -Math.Log(Random.Shared.NextDouble()) / ((double)a.Price / a.Types.Length * catCount / (string.IsNullOrEmpty(a.CategoryIds) ? catCount : (a.CategoryIds.Length + 1))))
                .Take(count));
            }

            var ids = list.Select(a => a.Id).ToArray();
            GetQuery(a => ids.Contains(a.Id)).ExecuteUpdate(p => p.SetProperty(a => a.DisplayCount, a => a.DisplayCount + 1));
            return list;
        }, TimeSpan.FromMinutes(10));
    }
}