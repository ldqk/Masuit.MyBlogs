using FreeRedis;

namespace Masuit.MyBlogs.Core.Common;

public static class RedisClientExtension
{
    public static long IncrBy(this IRedisClient client, string key, int inc, TimeSpan? expire = null)
    {
        using var multi = client.Multi();
        var incr = multi.IncrBy(key, inc);
        if (expire.HasValue)
        {
            multi.Expire(key, expire.Value);
        }

        multi.Exec();
        return incr;
    }

    public static long Incr(this IRedisClient client, string key, TimeSpan? expire = null)
    {
        using var multi = client.Multi();
        var incr = multi.Incr(key);
        if (expire.HasValue)
        {
            multi.Expire(key, expire.Value);
        }

        multi.Exec();
        return incr;
    }

    public static async Task<long> IncrByAsync(this IRedisClient client, string key, int inc, TimeSpan? expire = null)
    {
        using var multi = client.Multi();
        var incr = await multi.IncrByAsync(key, inc);
        if (expire.HasValue)
        {
            await multi.ExpireAsync(key, expire.Value);
        }

        multi.Exec();
        return incr;
    }

    public static async Task<long> IncrAsync(this IRedisClient client, string key, TimeSpan? expire = null)
    {
        using var multi = client.Multi();
        var incr = await multi.IncrAsync(key);
        if (expire.HasValue)
        {
            await multi.ExpireAsync(key, expire.Value);
        }

        multi.Exec();
        return incr;
    }

    public static T GetOrAdd<T>(this IRedisClient client, string key, T addValue, TimeSpan? expire = null)
    {
        return client.SetNx(key, addValue, expire ?? TimeSpan.FromSeconds(0)) ? addValue : client.Get<T>(key);
    }

    public static T GetOrAdd<T>(this IRedisClient client, string key, Func<T> addValue, TimeSpan? expire = null)
    {
        if (client.Exists(key))
        {
            return client.Get<T>(key);
        }

        var value = addValue();
        client.SetNx(key, value, expire ?? TimeSpan.FromSeconds(0));
        return value;
    }

    public static async Task<T> GetOrAddAsync<T>(this IRedisClient client, string key, T addValue, TimeSpan? expire = null)
    {
        return await client.SetNxAsync(key, addValue, expire ?? TimeSpan.FromSeconds(0)) ? addValue : await client.GetAsync<T>(key);
    }

    public static async Task<T> GetOrAddAsync<T>(this IRedisClient client, string key, Func<T> addValue, TimeSpan? expire = null)
    {
        if (await client.ExistsAsync(key))
        {
            return await client.GetAsync<T>(key);
        }

        var value = addValue();
        await client.SetNxAsync(key, value, expire ?? TimeSpan.FromSeconds(0));
        return value;
    }

    public static async Task<T> GetOrAddAsync<T>(this IRedisClient client, string key, Func<Task<T>> addValue, TimeSpan? expire = null)
    {
        if (await client.ExistsAsync(key))
        {
            return await client.GetAsync<T>(key);
        }

        var value = await addValue();
        await client.SetNxAsync(key, value, expire ?? TimeSpan.FromSeconds(0));
        return value;
    }

    public static T AddOrUpdate<T>(this IRedisClient client, string key, T addValue, T updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        if (client.SetNx(key, addValue, expire ?? TimeSpan.FromSeconds(0)))
        {
            return addValue;
        }

        var value = updateValue;
        if (isSliding && expire.HasValue)
        {
            client.Set(key, updateValue, expire.Value);
        }
        else
        {
            client.Set(key, updateValue, true);
        }

        return value;
    }

    public static T AddOrUpdate<T>(this IRedisClient client, string key, T addValue, Func<T> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        if (client.SetNx(key, addValue, expire ?? TimeSpan.FromSeconds(0)))
        {
            return addValue;
        }

        var update = updateValue();
        if (isSliding && expire.HasValue)
        {
            client.Set(key, update, expire.Value);
        }
        else
        {
            client.Set(key, update, true);
        }

        return update;
    }

    public static T AddOrUpdate<T>(this IRedisClient client, string key, T addValue, Func<T, T> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        if (client.SetNx(key, addValue, expire ?? TimeSpan.FromSeconds(0)))
        {
            return addValue;
        }

        var value = updateValue(client.Get<T>(key));
        if (isSliding && expire.HasValue)
        {
            client.Set(key, value, expire.Value);
        }
        else
        {
            client.Set(key, value, true);
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, T updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        if (await client.SetNxAsync(key, addValue, expire ?? TimeSpan.FromSeconds(0)))
        {
            return addValue;
        }

        if (isSliding && expire.HasValue)
        {
            await client.SetAsync(key, updateValue, expire.Value);
        }
        else
        {
            await client.SetAsync(key, updateValue, true);
        }

        return updateValue;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, Func<T> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        if (await client.SetNxAsync(key, addValue, expire ?? TimeSpan.FromSeconds(0)))
        {
            return addValue;
        }

        var value = updateValue();
        if (isSliding && expire.HasValue)
        {
            await client.SetAsync(key, value, expire.Value);
        }
        else
        {
            await client.SetAsync(key, value, true);
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, Func<T, T> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        if (await client.SetNxAsync(key, addValue, expire ?? TimeSpan.FromSeconds(0)))
        {
            return addValue;
        }

        var value = updateValue(await client.GetAsync<T>(key));
        if (isSliding && expire.HasValue)
        {
            await client.SetAsync(key, value, expire.Value);
        }
        else
        {
            await client.SetAsync(key, value, true);
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, Func<Task<T>> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        if (await client.SetNxAsync(key, addValue, expire ?? TimeSpan.FromSeconds(0)))
        {
            return addValue;
        }

        var value = await updateValue();
        if (isSliding && expire.HasValue)
        {
            await client.SetAsync(key, value, expire.Value);
        }
        else
        {
            await client.SetAsync(key, value, true);
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, Func<T, Task<T>> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        if (await client.SetNxAsync(key, addValue, expire ?? TimeSpan.FromSeconds(0)))
        {
            return addValue;
        }

        var value = await updateValue(await client.GetAsync<T>(key));
        if (isSliding && expire.HasValue)
        {
            await client.SetAsync(key, value, expire.Value);
        }
        else
        {
            await client.SetAsync(key, value, true);
        }

        return value;
    }

    /// <summary>
    /// 分布式锁
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="client"></param>
    /// <param name="key">锁对象</param>
    /// <param name="func">需要锁的代码</param>
    /// <param name="expire">锁过期时间</param>
    /// <returns>T</returns>
    public static T Lock<T>(this IRedisClient client, string key, Func<T> func, int expire = 60)
    {
        using (client.Lock(key, expire))
        {
            return func();
        }
    }

    /// <summary>
    /// 分布式锁
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="client"></param>
    /// <param name="key">锁对象</param>
    /// <param name="func">需要锁的代码</param>
    /// <param name="expire">锁过期时间</param>
    /// <returns>T</returns>
    public static Task<T> Lock<T>(this IRedisClient client, string key, Func<Task<T>> func, int expire = 60)
    {
        using (client.Lock(key, expire))
        {
            return func();
        }
    }

    /// <summary>
    /// 分布式锁
    /// </summary>
    /// <param name="client"></param>
    /// <param name="key">锁对象</param>
    /// <param name="action">需要锁的代码</param>
    /// <param name="expire">锁过期时间</param>
    public static void Lock(this IRedisClient client, string key, Action action, int expire = 60)
    {
        using (client.Lock(key, expire))
        {
            action();
        }
    }
}