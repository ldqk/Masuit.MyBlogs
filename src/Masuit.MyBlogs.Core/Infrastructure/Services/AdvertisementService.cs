using CacheManager.Core;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Z.EntityFramework.Plus;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public partial class AdvertisementService : BaseService<Advertisement>, IAdvertisementService
{
    public ICacheManager<List<Advertisement>> CacheManager { get; set; }

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
    public Advertisement GetByWeightedPrice(AdvertiseType type, IPLocation location, int? cid = null, string keywords = "")
    {
        return GetsByWeightedPrice(1, type, location, cid, keywords).FirstOrDefault();
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
    public List<Advertisement> GetsByWeightedPrice(int count, AdvertiseType type, IPLocation ipinfo, int? cid = null, string keywords = "")
    {
        var (location, _, _) = ipinfo;
        return CacheManager.GetOrAdd($"Advertisement:{location.Crc32()}:{type}:{count}-{cid}-{keywords}", _ =>
        {
            var atype = type.ToString("D");
            Expression<Func<Advertisement, bool>> where = a => a.Types.Contains(atype) && a.Status == Status.Available;
            var catCount = CategoryRepository.Count(_ => true);
            where = where.And(a => a.RegionMode == RegionLimitMode.All || (a.RegionMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase) : !Regex.IsMatch(location, a.Regions, RegexOptions.IgnoreCase)));
            if (cid.HasValue)
            {
                var pids = CategoryRepository.GetQuery(c => c.Id == cid).Select(c => c.ParentId + "|" + c.Parent.ParentId).FromCache(new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(5)
                }).ToArray();
                var scid = pids.Select(s => s.Trim('|')).Where(s => !string.IsNullOrEmpty(s)).Append(cid + "").Join("|");
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

            var array = GetQuery(a => a.Status == Status.Available).GroupBy(a => a.Merchant).Select(g => g.OrderBy(_ => EF.Functions.Random()).FirstOrDefault().Id).Take(50).ToArray();
            var list = GetQuery(where).Where(a => array.Contains(a.Id)).OrderBy(a => -Math.Log(EF.Functions.Random()) / ((double)a.Price / a.Types.Length * catCount / (string.IsNullOrEmpty(a.CategoryIds) ? catCount : (a.CategoryIds.Length + 1)))).Take(count).ToList();
            if (list.Count == 0 && keywords is { Length: > 0 })
            {
                return GetsByWeightedPrice(count, type, ipinfo, cid);
            }

            var ids = list.Select(a => a.Id).ToArray();
            GetQuery(a => ids.Contains(a.Id)).ExecuteUpdate(a => a.SetProperty(a => a.DisplayCount, a => a.DisplayCount + 1));
            return list;
        });
    }
}