using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire
{
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
        void LoginRecord(UserInfoOutputDto userInfo, string ip, LoginType type);

        /// <summary>
        /// 文章定时发表
        /// </summary>
        /// <param name="p"></param>
        void PublishPost(Post p);

        /// <summary>
        /// 文章访问记录
        /// </summary>
        /// <param name="pid"></param>
        void RecordPostVisit(int pid);

        /// <summary>
        /// 每日任务
        /// </summary>
        void EverydayJob();

        /// <summary>
        /// 友链检查
        /// </summary>
        void CheckLinks();

        /// <summary>
        /// 重建Lucene索引库
        /// </summary>
        void CreateLiceneIndex();
    }
}