using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using EFCoreSecondLevelCacheInterceptor;
using FreeRedis;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed class AdvertisementService : BaseService<Advertisement>, IAdvertisementService
{
    public IRedisClient CacheManager { get; set; }

    public ICategoryRepository CategoryRepository { get; set; }

    private readonly ILuceneIndexSearcher _luceneIndexSearcher;

    public AdvertisementService(IBaseRepository<Advertisement> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
    {
        _luceneIndexSearcher = searcher;
    }

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
        return CacheManager.GetOrAdd($"Advertisement:{location.Crc32()}:{type}:{count}-{cid}-{keywords}", () =>
        {
            var atype = type.ToString("D");
            Expression<Func<Advertisement, bool>> where = a => a.Types.Contains(atype) && a.Status == Status.Available;
            var catCount = CategoryRepository.Count(_ => true);
            where = where.And(a => a.RegionMode == RegionLimitMode.All || (a.RegionMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase) : !Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase)));
            if (cid.HasValue)
            {
                var pids = CategoryRepository.GetQuery(c => c.Id == cid).Select(c => string.Concat(c.ParentId, "|", c.Parent.ParentId).Trim('|')).Distinct().Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromHours(5)).ToList();
                var scid = pids.Append(cid + "").Join("|");
                if (Any(a => Regex.IsMatch(a.CategoryIds, scid)))
                {
                    where = where.And(a => Regex.IsMatch(a.CategoryIds, scid) || string.IsNullOrEmpty(a.CategoryIds));
                }
            }

            if (!keywords.IsNullOrEmpty())
            {
                var regex = _luceneIndexSearcher.CutKeywords(keywords).Select(Regex.Escape).Join("|");
                where = where.And(a => Regex.IsMatch(a.Title + a.Description, regex));
            }

            var array = GetQuery(a => a.Types.Contains(atype) && a.Status == Status.Available).GroupBy(a => a.Merchant).Select(g => g.OrderBy(_ => EF.Functions.Random()).FirstOrDefault().Id).Take(50).ToArray();
            var list = GetQuery<AdvertisementDto>(where).Where(a => array.Contains(a.Id)).OrderBy(a => -Math.Log(EF.Functions.Random()) / ((double)a.Price / a.Types.Length * catCount / (string.IsNullOrEmpty(a.CategoryIds) ? catCount : (a.CategoryIds.Length + 1)))).Take(count).ToList();
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
        return CacheManager.GetOrAdd($"Advertisement:{location.Crc32()}:{type}:{count}-{cid}-{keywords}", () =>
        {
            var all = GetQuery<AdvertisementDto>(a => a.Status == Status.Available).Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(10)).ToList();
            var atype = type.ToString("D");
            var catCount = CategoryRepository.Count(_ => true);
            string scid = "";
            if (cid.HasValue)
            {
                scid = CategoryRepository.GetQuery(c => c.Id == cid).Select(c => string.Concat(c.ParentId, "|", c.Parent.ParentId).Trim('|')).Distinct().Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromHours(5)).AsEnumerable().Append(cid + "").Join("|");
            }

            var array = all.Where(a => a.Types.Contains(atype)).GroupBy(a => a.Merchant).Select(g => g.OrderByRandom().FirstOrDefault().Id).Take(50).ToArray();
            var list = all.Where(a => a.Types.Contains(atype) && array.Contains(a.Id))
                .Where(a => a.RegionMode == RegionLimitMode.All || (a.RegionMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase) : !Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase)))
                .WhereIf(cid.HasValue, a => Regex.IsMatch(a.CategoryIds + "", scid) || string.IsNullOrEmpty(a.CategoryIds))
                .WhereIf(!keywords.IsNullOrEmpty(), a => (a.Title + a.Description).Contains(_luceneIndexSearcher.CutKeywords(keywords)))
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
