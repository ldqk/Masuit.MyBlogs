using FreeRedis;

namespace Masuit.MyBlogs.Core.Common;

public sealed class EmailVerificationCode(IRedisClient redisClient)
{
    public async Task<string> VerifyAsync(string email, string code, bool consume = false)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
        {
            return "请输入邮箱验证码！";
        }

        const string script = """
            local attempts = redis.call('INCR', KEYS[2])
            if attempts == 1 then redis.call('EXPIRE', KEYS[2], 600) end
            if attempts > 5 then return -1 end
            local expected = redis.call('GET', KEYS[1])
            if not expected or expected ~= ARGV[1] then return 0 end
            if ARGV[2] == '1' then redis.call('DEL', KEYS[1]) end
            return 1
            """;
        var result = Convert.ToInt32(await redisClient.EvalAsync(script, ["code:" + email, "code:attempts:" + email], [code, consume ? "1" : "0"]));
        return result switch
        {
            1 => string.Empty,
            -1 => "验证码尝试次数过多，请10分钟后再试！",
            _ => "验证码错误！"
        };
    }
}
