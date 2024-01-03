namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface
{
	public partial interface IAdvertisementService : IBaseService<Advertisement>
	{
		/// <summary>
		/// 按价格随机筛选一个元素
		/// </summary>
		/// <param name="type">广告类型</param>
		/// <param name="location"></param>
		/// <param name="cid">分类id</param>
		/// <param name="keywords"></param>
		/// <returns></returns>
		AdvertisementDto GetByWeightedPrice(AdvertiseType type, IPLocation location, int? cid = null, string keywords = "");

		/// <summary>
		/// 按价格随机筛选多个元素
		/// </summary>
		/// <param name="count">数量</param>
		/// <param name="type">广告类型</param>
		/// <param name="location"></param>
		/// <param name="cid">分类id</param>
		/// <param name="keywords"></param>
		/// <returns></returns>
		List<AdvertisementDto> GetsByWeightedPrice(int count, AdvertiseType type, IPLocation location, int? cid = null, string keywords = "");
	}
}
