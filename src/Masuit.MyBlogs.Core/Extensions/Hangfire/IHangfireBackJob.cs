using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire
{
    public interface IHangfireBackJob
    {
        void LoginRecord(UserInfoOutputDto userInfo, string ip, LoginType type);
        void PublishPost(Post p);
        void RecordPostVisit(int pid);
        void EverydayJob();
        void CheckLinks();
    }
}