using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// session扩展
    /// </summary>
    public static class SessionExt
    {
        /// <summary>
        /// 将Session存到Redis，需要先在config中配置链接字符串，连接字符串在config配置文件中的ConnectionStrings节下配置，name固定为RedisHosts，值的格式：127.0.0.1:6379,allowadmin=true，若未正确配置，将按默认值“127.0.0.1:6379,allowadmin=true”进行操作，如：<br/>
        /// &lt;connectionStrings&gt;<br/>
        ///      &lt;add name = "RedisHosts" connectionString="127.0.0.1:6379,allowadmin=true"/&gt;<br/>
        /// &lt;/connectionStrings&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <param name="obj">需要存的对象</param>
        /// <param name="expire">过期时间，默认20分钟</param>
        /// <returns></returns>
        public static void SetByRedis<T>(this ISession session, string key, T obj, int expire = 20)
        {
            if (HttpContext2.Current is null)
            {
                throw new Exception("请确保此方法调用是在同步线程中执行！");
            }
            session?.SetString(key, obj.ToJsonString());

            try
            {
                RedisHelper.HSet("Session:" + session.Id, key, obj); //存储数据到缓存服务器，这里将字符串"my value"缓存，key 是"test"
                RedisHelper.Expire("Session:" + session.Id, TimeSpan.FromMinutes(expire));
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// 从Redis取Session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <param name="expire">过期时间，默认20分钟</param>
        /// <returns></returns> 
        public static T GetByRedis<T>(this ISession session, string key, int expire = 20) where T : class
        {
            if (HttpContext2.Current is null)
            {
                throw new Exception("请确保此方法调用是在同步线程中执行！");
            }
            T obj = default(T);
            if (session != null)
            {
                obj = session.Get<T>(key);
            }

            if (obj == default(T))
            {
                try
                {
                    var sessionKey = "Session:" + session.Id;
                    if (RedisHelper.Exists(sessionKey) && RedisHelper.HExists(sessionKey, key))
                    {
                        RedisHelper.Expire(sessionKey, TimeSpan.FromMinutes(expire));
                        return RedisHelper.HGet<T>(sessionKey, key);
                    }

                    return default(T);
                }
                catch
                {
                    return default(T);
                }
            }

            return obj;
        }

        /// <summary>
        /// 从Redis移除对应键的Session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static void RemoveByRedis(this ISession session, string key)
        {
            if (HttpContext2.Current is null)
            {
                throw new Exception("请确保此方法调用是在同步线程中执行！");
            }

            session?.Remove(key);

            try
            {
                var sessionKey = "Session:" + session.Id;
                if (RedisHelper.Exists(sessionKey) && RedisHelper.HExists(sessionKey, key))
                {
                    RedisHelper.HDel(sessionKey, key);
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
        public static int SessionCount(this ISession session)
        {
            try
            {
                return RedisHelper.Keys("Session:*").Count();
            }
            catch (Exception e)
            {
                LogManager.Error(e);
                return 0;
            }
        }
    }
}