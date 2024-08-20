using FreeRedis;
using Masuit.Tools.Logging;

namespace Masuit.MyBlogs.Core.Extensions;

public static class SessionExtension
{
    private static IRedisClient _redis;

    public static IApplicationBuilder UseRedisSession(this IApplicationBuilder app)
    {
        _redis = app.ApplicationServices.GetRequiredService<IRedisClient>();
        return app;
    }

    #region 写Session

    /// <summary>
    /// 将Session存到Redis，需要先在config中配置链接字符串，连接字符串在config配置文件中的ConnectionStrings节下配置，name固定为RedisHosts，值的格式：127.0.0.1:6379,allowadmin=true，若未正确配置，将按默认值“127.0.0.1:6379,allowadmin=true”进行操作，如：<br/>
    /// &lt;connectionStrings&gt;<br/>
    ///      &lt;add name = "RedisHosts" connectionString="127.0.0.1:6379,allowadmin=true"/&gt;<br/>
    /// &lt;/connectionStrings&gt;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="key">键</param>
    /// <param name="obj">需要存的对象</param>
    /// <param name="expire">过期时间，默认20分钟</param>
    /// <returns></returns>
    public static void SetRedisSession<T>(this HttpContext context, string key, T obj, int expire = 20)
    {
        var sessionKey = context.Request.Cookies["SessionID"];
        if (string.IsNullOrEmpty(sessionKey))
        {
            sessionKey = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("SessionID", sessionKey);
        }

        context.Session.Set(key, obj);
        try
        {
            _redis.HSet("Session:" + sessionKey, key, obj);
            _redis.Expire("Session:" + sessionKey, TimeSpan.FromMinutes(expire));
        }
        catch
        {
            //ignore
        }
    }

    #endregion

    #region 获取Session
    public static string GetRedisSessionKey(this HttpContext context)
    {
        return context.Request.Cookies["SessionID"];
    }

    /// <summary>
    /// 从Redis取Session
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="key">键</param>
    /// <param name="expire">过期时间，默认20分钟</param>
    /// <returns></returns> 
    public static T GetRedisSession<T>(this HttpContext context, string key, int expire = 20) where T : class
    {
        var sessionKey = context.Request.Cookies["SessionID"];
        if (string.IsNullOrEmpty(sessionKey))
        {
            return default;
        }

        T obj = context.Session.Get<T>(key);
        if (obj != null)
        {
            return obj;
        }

        try
        {
            sessionKey = "Session:" + sessionKey;
            if (_redis.Exists(sessionKey) && _redis.HExists(sessionKey, key))
            {
                _redis.Expire(sessionKey, TimeSpan.FromMinutes(expire));
                return _redis.HGet<T>(sessionKey, key);
            }

            return default;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 从Redis移除对应键的Session
    /// </summary>
    /// <param name="_"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static void RemoveRedisSession(this HttpContext context, string key)
    {
        var sessionKey = context.Request.Cookies["SessionID"];
        if (string.IsNullOrEmpty(sessionKey))
        {
            return;
        }

        try
        {
            context.Session.Remove(key);
            sessionKey = "Session:" + sessionKey;
            if (_redis.Exists(sessionKey) && _redis.HExists(sessionKey, key))
            {
                _redis.HDel(sessionKey, key);
            }
        }
        catch (Exception e)
        {
            LogManager.Error(e);
        }
    }

    /// <summary>
    /// Session个数
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public static int SessionCount(this HttpContext context)
    {
        try
        {
            return _redis.Keys("Session:*").Count();
        }
        catch (Exception e)
        {
            LogManager.Error(e);
            return 0;
        }
    }

    #endregion
}