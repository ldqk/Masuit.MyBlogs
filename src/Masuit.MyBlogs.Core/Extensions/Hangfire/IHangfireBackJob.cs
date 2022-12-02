namespace Masuit.MyBlogs.Core.Extensions.Hangfire;

/// <summary>
/// hangfire后台任务
/// </summary>
public interface IHangfireBackJob
{
	/// <summary>
	/// 登陆记录
	/// </summary>
	/// <param name="userInfo"></param>
	/// <param name="ip"></param>
	/// <param name="type"></param>
	void LoginRecord(UserInfoDto userInfo, string ip, LoginType type);

	/// <summary>
	/// 文章定时发表
	/// </summary>
	/// <param name="p"></param>
	void PublishPost(Post p);

	/// <summary>
	/// 文章访问记录
	/// </summary>
	/// <param name="pid"></param>
	/// <param name="ip"></param>
	/// <param name="refer"></param>
	/// <param name="url"></param><param name="headers"></param>
	void RecordPostVisit(int pid, string ip, string refer, string url);

	/// <summary>
	/// 每日任务
	/// </summary>
	void EverydayJob();

	/// <summary>
	/// 每月的任务
	/// </summary>
	void EverymonthJob();

	/// <summary>
	/// 友链检查
	/// </summary>
	void CheckLinks();

	void CheckAdvertisements();

	/// <summary>
	/// 更新友链权重
	/// </summary>
	/// <param name="referer"></param>
	/// <param name="ip"></param>
	void UpdateLinkWeight(string referer, string ip);

	/// <summary>
	/// 重建Lucene索引库
	/// </summary>
	void CreateLuceneIndex();

	/// <summary>
	/// 搜索统计
	/// </summary>
	void StatisticsSearchKeywords();
}