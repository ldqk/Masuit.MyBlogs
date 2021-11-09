using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface
{
    public partial interface IAdvertisementService : IBaseService<Advertisement>
    {
        /// <summary>
        /// 按价格随机筛选一个元素
        /// </summary>
        /// <param name="type">广告类型</param>
        /// <param name="cid">分类id</param>
        /// <returns></returns>
        Advertisement GetByWeightedPrice(AdvertiseType type, IPLocation location, int? cid = null);

        /// <summary>
        /// 按价格随机筛选多个元素
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="type">广告类型</param>
        /// <param name="cid">分类id</param>
        /// <returns></returns>
        List<Advertisement> GetsByWeightedPrice(int count, AdvertiseType type, IPLocation location, int? cid = null);
    }
}