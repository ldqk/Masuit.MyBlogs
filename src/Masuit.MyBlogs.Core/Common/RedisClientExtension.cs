using FreeRedis;

namespace Masuit.MyBlogs.Core.Common;

public static class RedisClientExtension
{
    public static long IncrBy(this IRedisClient client, string key, int inc, TimeSpan? expire = null)
    {
        var incr = client.IncrBy(key, inc);
        if (expire.HasValue)
        {
            client.Expire(key, expire.Value);
        }

        return incr;
    }

    public static long Incr(this IRedisClient client, string key, TimeSpan? expire = null)
    {
        var incr = client.Incr(key);
        if (expire.HasValue)
        {
            client.Expire(key, expire.Value);
        }

        return incr;
    }

    public static async Task<long> IncrByAsync(this IRedisClient client, string key, int inc, TimeSpan? expire = null)
    {
        var incr = await client.IncrByAsync(key, inc);
        if (expire.HasValue)
        {
            await client.ExpireAsync(key, expire.Value);
        }

        return incr;
    }

    public static async Task<long> IncrAsync(this IRedisClient client, string key, TimeSpan? expire = null)
    {
        var incr = await client.IncrAsync(key);
        if (expire.HasValue)
        {
            await client.ExpireAsync(key, expire.Value);
        }

        return incr;
    }

    public static T GetOrAdd<T>(this IRedisClient client, string key, T addValue, TimeSpan? expire = null)
    {
        var value = client.Get<CacheEntry<T>>(key);
        if (value is null)
        {
            client.Set(key, new CacheEntry<T>(addValue));
            if (expire.HasValue)
            {
                client.Expire(key, expire.Value);
            }
            return addValue;
        }
        return value;
    }

    public static T GetOrAdd<T>(this IRedisClient client, string key, Func<T> addValue, TimeSpan? expire = null)
    {
        var value = client.Get<CacheEntry<T>>(key);
        if (value is null)
        {
            value = new CacheEntry<T>(addValue());
            client.Set(key, value);
            if (expire.HasValue)
            {
                client.Expire(key, expire.Value);
            }
        }
        return value;
    }

    public static async Task<T> GetOrAddAsync<T>(this IRedisClient client, string key, T addValue, TimeSpan? expire = null)
    {
        var value = await client.GetAsync<CacheEntry<T>>(key);
        if (value is null)
        {
            await client.SetAsync(key, new CacheEntry<T>(addValue));
            if (expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
            return addValue;
        }
        return value;
    }

    public static async Task<T> GetOrAddAsync<T>(this IRedisClient client, string key, Func<T> addValue, TimeSpan? expire = null)
    {
        var value = await client.GetAsync<CacheEntry<T>>(key);
        if (value is null)
        {
            value = new CacheEntry<T>(addValue());
            await client.SetAsync(key, value);
            if (expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }
        return value;
    }

    public static async Task<T> GetOrAddAsync<T>(this IRedisClient client, string key, Func<Task<T>> addValue, TimeSpan? expire = null)
    {
        var value = await client.GetAsync<CacheEntry<T>>(key);
        if (value is null)
        {
            value = new CacheEntry<T>(await addValue());
            await client.SetAsync(key, value);
            if (expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }
        return value;
    }

    public static T AddOrUpdate<T>(this IRedisClient client, string key, T addValue, T updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        T value;
        if (client.Exists(key))
        {
            value = new CacheEntry<T>(updateValue);
            client.Set(key, value);
            if (isSliding && expire.HasValue)
            {
                client.Expire(key, expire.Value);
            }
        }
        else
        {
            value = new CacheEntry<T>(addValue);
            client.Set(key, value);
            if (expire.HasValue)
            {
                client.Expire(key, expire.Value);
            }
        }

        return value;
    }

    public static T AddOrUpdate<T>(this IRedisClient client, string key, T addValue, Func<T> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        T value;
        if (client.Exists(key))
        {
            var update = updateValue();
            client.Set(key, new CacheEntry<T>(update));
            value = update;
            if (isSliding && expire.HasValue)
            {
                client.Expire(key, expire.Value);
            }
        }
        else
        {
            client.Set(key, new CacheEntry<T>(addValue));
            value = addValue;
            if (expire.HasValue)
            {
                client.Expire(key, expire.Value);
            }
        }

        return value;
    }

    public static T AddOrUpdate<T>(this IRedisClient client, string key, T addValue, Func<T, T> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        var value = client.Get<CacheEntry<T>>(key);
        if (value is null)
        {
            client.Set(key, new CacheEntry<T>(addValue));
            value = addValue;
            if (expire.HasValue)
            {
                client.Expire(key, expire.Value);
            }
        }
        else
        {
            value = updateValue(value);
            client.Set(key, new CacheEntry<T>(value));
            if (isSliding && expire.HasValue)
            {
                client.Expire(key, expire.Value);
            }
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, T updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        T value;
        if (await client.ExistsAsync(key))
        {
            await client.SetAsync(key, new CacheEntry<T>(updateValue));
            value = updateValue;
            if (isSliding && expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }
        else
        {
            await client.SetAsync(key, new CacheEntry<T>(addValue));
            value = addValue;
            if (expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, Func<T> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        T value;
        if (await client.ExistsAsync(key))
        {
            var update = updateValue();
            await client.SetAsync(key, new CacheEntry<T>(update));
            value = update;
            if (isSliding && expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }
        else
        {
            await client.SetAsync(key, new CacheEntry<T>(addValue));
            value = addValue;
            if (expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, Func<T, T> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        var value = await client.GetAsync<CacheEntry<T>>(key);
        if (value is null)
        {
            await client.SetAsync(key, new CacheEntry<T>(addValue));
            value = addValue;
            if (expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }
        else
        {
            var update = updateValue(value);
            await client.SetAsync(key, new CacheEntry<T>(update));
            value = update;
            if (isSliding && expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, Func<Task<T>> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        T value;
        if (!await client.ExistsAsync(key))
        {
            await client.SetAsync(key, new CacheEntry<T>(addValue));
            value = addValue;
            if (expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }
        else
        {
            var update = await updateValue();
            await client.SetAsync(key, new CacheEntry<T>(update));
            value = update;
            if (isSliding && expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }

        return value;
    }

    public static async Task<T> AddOrUpdateAsync<T>(this IRedisClient client, string key, T addValue, Func<T, Task<T>> updateValue, TimeSpan? expire = null, bool isSliding = true)
    {
        var value = await client.GetAsync<CacheEntry<T>>(key);
        if (value is null)
        {
            await client.SetAsync(key, new CacheEntry<T>(addValue));
            value = addValue;
            if (expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }
        else
        {
            var update = await updateValue(value);
            await client.SetAsync(key, new CacheEntry<T>(update));
            value = update;
            if (isSliding && expire.HasValue)
            {
                await client.ExpireAsync(key, expire.Value);
            }
        }

        return value;
    }

    /// <summary>
    /// 创建并获取分布式锁对象
    /// </summary>
    /// <param name="client"></param>
    /// <param name="key">锁对象</param>
    /// <param name="expire">锁过期时间</param>
    /// <returns>T</returns>
    public static RedisClient.LockController Lock(this IRedisClient client, string key, int expire = 60) => client.Lock(key, expire);

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

/// <summary>
/// 缓存实体
/// </summary>
/// <typeparam name="T"></typeparam>
public record CacheEntry<T>
{
    public CacheEntry()
    {
    }

    public CacheEntry(T value)
    {
        Value = value;
    }

    public T Value { get; set; }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="entry"></param>
    public static implicit operator T(CacheEntry<T> entry)
    {
        return entry.Value;
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="item"></param>
    public static implicit operator CacheEntry<T>(T item)
    {
        return new CacheEntry<T>(item);
    }
}
