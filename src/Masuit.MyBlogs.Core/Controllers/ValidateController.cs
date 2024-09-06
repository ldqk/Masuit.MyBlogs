using Hangfire;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.Core.Validator;

namespace Masuit.MyBlogs.Core.Controllers;

public sealed class ValidateController : BaseController
{
    /// <summary>
    /// 发送验证码
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost, ValidateAntiForgeryToken, ResponseCache(Duration = 115, VaryByQueryKeys = new[] { "email" }), DistributedLockFilter]
    public ActionResult SendCode(string email)
    {
        var validator = new IsEmailAttribute();
        if (!validator.IsValid(email))
        {
            return ResultData(null, false, validator.ErrorMessage);
        }

        if (RedisHelper.Exists("get:" + email))
        {
            RedisHelper.Expire("get:" + email, 120);
            return ResultData(null, false, "发送频率限制，请在2分钟后重新尝试发送邮件！请检查你的邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！");
        }

        string code = SnowFlake.GetInstance().GetUniqueShortId(6);
        RedisHelper.Set("code:" + email, code, 86400);
        BackgroundJob.Enqueue<IMailSender>(sender => sender.Send(Request.Host + "博客验证码", $"{Request.Host}本次验证码是：<span style='color:red'>{code}</span>，有效期为24h，请按时使用！", email, ClientIP.ToString()));
        RedisHelper.Set("get:" + email, code, 120);
#if !DEBUG
        return ResultData(null, true, "验证码发送成功！");
#else
        return ResultData(null, true, "验证码：" + code);
#endif
    }
}