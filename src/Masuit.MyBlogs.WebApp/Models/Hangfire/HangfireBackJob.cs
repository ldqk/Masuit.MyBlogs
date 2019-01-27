using Common;
using IBLL;
using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Masuit.Tools.Net;
using Masuit.Tools.NoSQL;
using Masuit.Tools.Win32;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using Models.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Masuit.MyBlogs.WebApp.Models.Hangfire
{
    public class HangfireBackJob : IHangfireBackJob
    {
        //public IInterviewBll InterviewBll { get; set; }
        public ISystemSettingBll SystemSettingBll { get; set; }
        public IUserInfoBll UserInfoBll { get; set; }
        public IPostBll PostBll { get; set; }
        public RedisHelper RedisHelper { get; set; }

        public HangfireBackJob(ISystemSettingBll systemSettingBll, IUserInfoBll userInfoBll, IPostBll postBll, RedisHelper redisHelper)
        {
            //InterviewBll = bll;
            SystemSettingBll = systemSettingBll;
            UserInfoBll = userInfoBll;
            PostBll = postBll;
            RedisHelper = redisHelper;
        }
        public void LoginRecord(UserInfoOutputDto userInfo, string ip, LoginType type)
        {
            var address = ip.GetPhysicsAddressInfo().Result;
            string addr = address.AddressResult.FormattedAddress;
            string prov = address.AddressResult.AddressComponent.Province;
            LoginRecord record = new LoginRecord()
            {
                IP = ip,
                LoginTime = DateTime.Now,
                LoginType = type,
                PhysicAddress = addr,
                Province = prov
            };
            UserInfo u = UserInfoBll.GetByUsername(userInfo.Username);
            u.LoginRecord.Add(record);
            UserInfoBll.UpdateEntitySaved(u);
            string content = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "template\\login.html").Replace("{{name}}", u.Username).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{ip}}", record.IP).Replace("{{address}}", record.PhysicAddress);
            CommonHelper.SendMail(CommonHelper.GetSettings("Title") + "账号登录通知", content, CommonHelper.GetSettings("ReceiveEmail"));
        }

        public void PublishPost(Post p)
        {
            p.Status = Status.Pended;
            p.PostDate = DateTime.Now;
            p.ModifyDate = DateTime.Now;
            PostBll.AddOrUpdateSaved(e => e.Id, p);
        }

        public void FlushException(Exception ex)
        {
            LogManager.Error(ex);
        }

        public void RecordPostVisit(int pid)
        {
            Post post = PostBll.GetById(pid);
            var record = post.PostAccessRecord.FirstOrDefault(r => r.AccessTime == DateTime.Today);
            if (record != null)
            {
                record.ClickCount += 1;
            }
            else
            {
                post.PostAccessRecord.Add(new PostAccessRecord
                {
                    ClickCount = 1,
                    AccessTime = DateTime.Today
                });
            }

            PostBll.UpdateEntitySaved(post);
        }

        public static void InterceptLog(IpIntercepter s)
        {
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                redisHelper.StringIncrement("interceptCount");
                redisHelper.ListLeftPush("intercept", s);
            }
        }
    }
}