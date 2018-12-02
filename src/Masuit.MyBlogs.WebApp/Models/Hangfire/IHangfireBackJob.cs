using Models.DTO;
using Models.Entity;
using Models.Enum;
using System;

namespace Masuit.MyBlogs.WebApp.Models.Hangfire
{
    public interface IHangfireBackJob
    {
        void FlushException(Exception ex);
        void FlushInetAddress(Interview interview);
        //void FlushUnhandledAddress();
        void UpdateLucene();
        void ResetLucene();
        void LoginRecord(UserInfoOutputDto userInfo, string ip, LoginType type);
        void PublishPost(Post p);
        void AggregateInterviews();
        void InterviewTrace(Guid uid, string url);
        void RecordPostVisit(int pid);
    }
}