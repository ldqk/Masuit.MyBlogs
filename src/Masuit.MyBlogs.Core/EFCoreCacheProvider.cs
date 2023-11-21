using EFCoreSecondLevelCacheInterceptor;
using FreeRedis;
using Masuit.MyBlogs.Core.Common;

namespace Masuit.MyBlogs.Core;

public class EFCoreCacheProvider : IEFCacheServiceProvider
{
    private readonly IRedisClient _redisClient;

    private readonly ILogger<EFCoreCacheProvider> _cacheLogger;
    private readonly IEFDebugLogger _logger;

    public EFCoreCacheProvider(IRedisClient redisClient, ILogger<EFCoreCacheProvider> cacheLogger, IEFDebugLogger logger)
    {
        _redisClient = redisClient;
        _cacheLogger = cacheLogger;
        _logger = logger;
    }

    /// <summary>
    ///     Adds a new item to the cache.
    /// </summary>
    /// <param name="cacheKey">key</param>
    /// <param name="value">value</param>
    /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
    public void InsertValue(EFCacheKey cacheKey, EFCachedData value, EFCachePolicy cachePolicy)
    {
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        value ??= new EFCachedData
        {
            IsNull = true
        };

        var keyHash = cacheKey.KeyHash;

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            if (string.IsNullOrWhiteSpace(rootCacheKey))
            {
                continue;
            }
            _redisClient.SAdd(rootCacheKey, keyHash);
            _redisClient.Expire(rootCacheKey, 3600);
        }

        if (cachePolicy == null)
        {
            _redisClient.Set(keyHash, value, 300);
        }
        else
        {
            _redisClient.AddOrUpdate(keyHash, value, value, cachePolicy.CacheTimeout, cachePolicy.CacheExpirationMode == CacheExpirationMode.Sliding);
        }
    }

    /// <summary>Removes the cached entries added by this library.</summary>
    public void ClearAllCachedEntries()
    {
        _redisClient.Del("EFCache:*");
    }

    /// <summary>
    ///     Gets a cached entry by key.
    /// </summary>
    /// <param name="cacheKey">key to find</param>
    /// <returns>cached value</returns>
    /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
    public EFCachedData GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
    {
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        return _redisClient.Get<EFCachedData>(cacheKey.KeyHash);
    }

    /// <summary>
    ///     Invalidates all of the cache entries which are dependent on any of the specified root keys.
    /// </summary>
    /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
    public void InvalidateCacheDependencies(EFCacheKey cacheKey)
    {
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            if (string.IsNullOrWhiteSpace(rootCacheKey))
            {
                continue;
            }

            var cachedValue = _redisClient.Get<EFCachedData>(cacheKey.KeyHash);
            var dependencyKeys = _redisClient.SMembers(rootCacheKey);
            if (dependencyKeys.IsNullOrEmpty() && cachedValue is not null)
            {
                if (_logger.IsLoggerEnabled)
                {
                    _cacheLogger.LogDebug(CacheableEventId.QueryResultInvalidated, "Invalidated all of the cache entries due to early expiration of a root cache key[{RootCacheKey}].", rootCacheKey);
                }

                _redisClient.Del(rootCacheKey);
                return;
            }
            _redisClient.Del(rootCacheKey);
        }
    }
}
