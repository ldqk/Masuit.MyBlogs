using Models.DTO;
using Models.Entity;
using Models.Enum;
using System;

namespace Masuit.MyBlogs.WebApp.Models.Hangfire
{
    public interface IHangfireBackJob
    {
        void FlushException(Exception ex);
        void LoginRecord(UserInfoOutputDto userInfo, string ip, LoginType type);
        void PublishPost(Post p);
        void RecordPostVisit(int pid);
    }
}