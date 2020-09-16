using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.Tools;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    public class ValidateController : BaseController
    {
        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, ResponseCache(Duration = 115, VaryByQueryKeys = new[] { "email" })]
        public async Task<ActionResult> SendCode(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.MatchEmail().isMatch)
            {
                return ResultData(null, false, "请输入正确的邮箱！");
            }

            if (await RedisHelper.ExistsAsync("get:" + email))
            {
                await RedisHelper.ExpireAsync("get:" + email, 120);
                return ResultData(null, false, "发送频率限制，请在2分钟后重新尝试发送邮件！请检查你的邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！");
            }

            string code = SnowFlake.GetInstance().GetUniqueShortId(6);
            await RedisHelper.SetAsync("code:" + email, code, 86400);
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "博客验证码", $"{Request.Host}本次验证码是：<span style='color:red'>{code}</span>，有效期为24h，请按时使用！", email, ClientIP));
            await RedisHelper.SetAsync("get:" + email, code, 120);
#if !DEBUG
            return ResultData(null, true, "验证码发送成功！");
#else
            return ResultData(null, true, "验证码：" + code);
#endif
        }
    }
}