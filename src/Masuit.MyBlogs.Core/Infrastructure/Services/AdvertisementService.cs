using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.LuceneEFCore.SearchEngine.Linq;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class AdvertisementService : BaseService<Advertisement>, IAdvertisementService
    {
        public AdvertisementService(IBaseRepository<Advertisement> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }

        /// <summary>
        /// 按权重随机筛选一个元素
        /// </summary>
        /// <param name="type">广告类型</param>
        /// <param name="cid">分类id</param>
        /// <returns></returns>
        public Advertisement GetByWeightedRandom(AdvertiseType type, int? cid = null)
        {
            return GetsByWeightedRandom(1, type, cid).FirstOrDefault();
        }

        /// <summary>
        /// 按权重随机筛选一个元素
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="type">广告类型</param>
        /// <param name="cid">分类id</param>
        /// <returns></returns>
        public List<Advertisement> GetsByWeightedRandom(int count, AdvertiseType type, int? cid = null)
        {
            Expression<Func<Advertisement, bool>> where = a => a.Types.Contains(type.ToString("D")) && a.Status == Status.Available;
            if (cid.HasValue)
            {
                where = where.And(a => a.CategoryId == cid || a.CategoryId == null);
            }

            return GetRandomWeightList(GetQueryFromCache(where).ToList(), count);
        }

        /// <summary>
        /// 按价格随机筛选一个元素
        /// </summary>
        /// <param name="type">广告类型</param>
        /// <param name="cid">分类id</param>
        /// <returns></returns>
        public Advertisement GetByWeightedPrice(AdvertiseType type, int? cid = null)
        {
            return GetsByWeightedPrice(1, type, cid).FirstOrDefault();
        }

        /// <summary>
        /// 按价格随机筛选一个元素
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="type">广告类型</param>
        /// <param name="cid">分类id</param>
        /// <returns></returns>
        public List<Advertisement> GetsByWeightedPrice(int count, AdvertiseType type, int? cid = null)
        {
            Expression<Func<Advertisement, bool>> where = a => a.Types.Contains(type.ToString("D")) && a.Status == Status.Available;
            if (cid.HasValue)
            {
                where = where.And(a => a.CategoryId == cid || a.CategoryId == null);
            }

            var list = GetQueryFromCache(@where).ToList();
            return GetRandomPriceList(list, count);
        }

        /// <summary>
        /// 产生一个带权重的随机数
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private int GenerateWeightedRandom(HashSet<int> list)
        {
            var num = list.FirstOrDefault(i => new Random().Next(1, list.Sum()) - i <= 0);
            if (list.Any() && list.All(i => i != num))
            {
                num = GenerateWeightedRandom(list);
            }

            return num;
        }

        /// <summary>
        /// 算法：
        /// 1.每个广告项权重+1命名为w，防止为0情况。
        /// 2.计算出总权重n。
        /// 3.每个广告项权重w加上从0到(n-1)的一个随机数（即总权重以内的随机数），得到新的权重排序值s。
        /// 4.根据得到新的权重排序值s进行排序，取前面s最大几个。
        /// </summary>
        /// <param name="list">原始列表</param>
        /// <param name="count">随机抽取条数</param>
        /// <returns></returns>
        private List<Advertisement> GetRandomWeightList(List<Advertisement> list, int count)
        {
            if (list.Count <= count || count <= 0)
            {
                return list;
            }

            //随机赋值权重
            var wlist = list.Select(t => t.Weight + 1 + new Random(GetRandomSeed()).Next(0, list.Sum(t => t.Weight + 1))).Select((w, i) => new KeyValuePair<int, int>(i, w)).ToList(); //第一个int为list下标索引、第一个int为权重排序值

            //排序
            wlist.Sort((kvp1, kvp2) => kvp2.Value - kvp1.Value);

            //根据实际情况取排在最前面的几个
            var newList = new List<Advertisement>();
            for (int i = 0; i < count; i++)
            {
                newList.Add(list[wlist[i].Key]);
            }

            //随机法则
            return newList;
        }

        /// <summary>
        /// 算法：
        /// 1.每个广告项权重+1命名为w，防止为0情况。
        /// 2.计算出总权重n。
        /// 3.每个广告项权重w加上从0到(n-1)的一个随机数（即总权重以内的随机数），得到新的权重排序值s。
        /// 4.根据得到新的权重排序值s进行排序，取前面s最大几个。
        /// </summary>
        /// <param name="list">原始列表</param>
        /// <param name="count">随机抽取条数</param>
        /// <returns></returns>
        private List<Advertisement> GetRandomPriceList(List<Advertisement> list, int count)
        {
            if (list.Count <= count || count <= 0)
            {
                return list;
            }

            //随机赋值权重
            var wlist = list.Select(t => t.Price + 1 + new Random(GetRandomSeed()).Next(0, list.Sum(t => (int)t.Price + 1))).Select((w, i) => new KeyValuePair<int, decimal>(i, w)).ToList(); //第一个int为list下标索引、第一个int为权重排序值

            //排序
            wlist.Sort((kvp1, kvp2) => (int)(kvp2.Value - kvp1.Value));

            //根据实际情况取排在最前面的几个
            var newList = new List<Advertisement>();
            for (int i = 0; i < count; i++)
            {
                newList.Add(list[wlist[i].Key]);
            }

            //随机法则
            return newList;
        }

        /// <summary>
        /// 随机种子值
        /// </summary>
        /// <returns></returns>
        private static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}