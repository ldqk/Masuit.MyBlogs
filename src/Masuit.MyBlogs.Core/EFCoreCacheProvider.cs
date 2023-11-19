using EFCoreSecondLevelCacheInterceptor;
using FreeRedis;
using Masuit.MyBlogs.Core.Common;

namespace Masuit.MyBlogs.Core;

public class EFCoreCacheProvider : IEFCacheServiceProvider
{
    private readonly IRedisClient _redisClient;

    private readonly ILogger<EFCacheManagerCoreProvider> _cacheManagerCoreProviderLogger;
    private readonly IEFDebugLogger _logger;

    public EFCoreCacheProvider(IRedisClient redisClient, ILogger<EFCacheManagerCoreProvider> cacheManagerCoreProviderLogger, IEFDebugLogger logger)
    {
        _redisClient = redisClient;
        _cacheManagerCoreProviderLogger = cacheManagerCoreProviderLogger;
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

        if (value == null)
        {
            value = new EFCachedData
            {
                IsNull = true
            };
        }

        var keyHash = cacheKey.KeyHash;

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            _redisClient.SAdd(rootCacheKey, keyHash);
        }

        if (cachePolicy == null)
        {
            _redisClient.Set(keyHash, value);
        }
        else
        {
            _redisClient.AddOrUpdate(keyHash, value, value, cachePolicy.CacheTimeout, cachePolicy.CacheExpirationMode == CacheExpirationMode.Sliding);
        }
    }

    /// <summary>
    ///     Removes the cached entries added by this library.
    /// </summary>
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
            if (AreRootCacheKeysExpired(cachedValue, dependencyKeys))
            {
                if (_logger.IsLoggerEnabled)
                {
                    _cacheManagerCoreProviderLogger.LogDebug(CacheableEventId.QueryResultInvalidated, "Invalidated all of the cache entries due to early expiration of a root cache key[{RootCacheKey}].", rootCacheKey);
                }

                ClearAllCachedEntries();
                return;
            }

            ClearDependencyValues(dependencyKeys);
            _redisClient.Del(rootCacheKey);
        }
    }

    private void ClearDependencyValues(string[] dependencyKeys)
    {
        if (dependencyKeys is null)
        {
            return;
        }

        foreach (var dependencyKey in dependencyKeys)
        {
            _redisClient.SRem(dependencyKey);
        }
    }

    private static bool AreRootCacheKeysExpired(EFCachedData cachedValue, string[] dependencyKeys) => cachedValue is not null && dependencyKeys is null;
}
