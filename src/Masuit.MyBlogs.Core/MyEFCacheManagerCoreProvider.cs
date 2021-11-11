using CacheManager.Core;
using EFCoreSecondLevelCacheInterceptor;

namespace Masuit.MyBlogs.Core
{
    /// <summary>
    /// Using ICacheManager as a cache service.
    /// </summary>
    public class MyEFCacheManagerCoreProvider : IEFCacheServiceProvider
    {
        private readonly IReaderWriterLockProvider _readerWriterLockProvider;
        private readonly ICacheManager<ISet<string>> _dependenciesCacheManager;
        private readonly ICacheManager<EFCachedData> _valuesCacheManager;
        private readonly string _keyPrefix = "EFCache:";
        /// <summary>
        /// Using IMemoryCache as a cache service.
        /// </summary>
        public MyEFCacheManagerCoreProvider(ICacheManager<ISet<string>> dependenciesCacheManager, ICacheManager<EFCachedData> valuesCacheManager, IReaderWriterLockProvider readerWriterLockProvider)
        {
            _readerWriterLockProvider = readerWriterLockProvider;
            _dependenciesCacheManager = dependenciesCacheManager ?? throw new ArgumentNullException(nameof(dependenciesCacheManager), "Please register the `ICacheManager`.");
            _valuesCacheManager = valuesCacheManager ?? throw new ArgumentNullException(nameof(valuesCacheManager), "Please register the `ICacheManager`.");
            _dependenciesCacheManager.OnRemoveByHandle += (sender, args) => ClearAllCachedEntries();
        }

        /// <summary>
        /// Adds a new item to the cache.
        /// </summary>
        /// <param name="cacheKey">key</param>
        /// <param name="value">value</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public void InsertValue(EFCacheKey cacheKey, EFCachedData value, EFCachePolicy cachePolicy)
        {
            _readerWriterLockProvider.TryWriteLocked(() =>
            {
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
                    _dependenciesCacheManager.AddOrUpdate(_keyPrefix + rootCacheKey, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        keyHash
                    }, updateValue: set =>
                    {
                        set.Add(keyHash);
                        return set;
                    });
                }

                if (cachePolicy == null)
                {
                    _valuesCacheManager.Add(_keyPrefix + keyHash, value);
                }
                else
                {
                    _valuesCacheManager.Add(new CacheItem<EFCachedData>(_keyPrefix + keyHash, value, cachePolicy.CacheExpirationMode == CacheExpirationMode.Absolute ? ExpirationMode.Absolute : ExpirationMode.Sliding, cachePolicy.CacheTimeout));
                }
            });
        }

        /// <summary>
        /// Removes the cached entries added by this library.
        /// </summary>
        public void ClearAllCachedEntries()
        {
            _readerWriterLockProvider.TryWriteLocked(() =>
            {
                _valuesCacheManager.Clear();
                _dependenciesCacheManager.Clear();
            });
        }

        /// <summary>
        /// Gets a cached entry by key.
        /// </summary>
        /// <param name="cacheKey">key to find</param>
        /// <returns>cached value</returns>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public EFCachedData GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
        {
            return _readerWriterLockProvider.TryReadLocked(() => _valuesCacheManager.Get<EFCachedData>(_keyPrefix + cacheKey.KeyHash));
        }

        /// <summary>
        /// Invalidates all of the cache entries which are dependent on any of the specified root keys.
        /// </summary>
        /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
        public void InvalidateCacheDependencies(EFCacheKey cacheKey)
        {
            _readerWriterLockProvider.TryWriteLocked(() =>
            {
                foreach (var rootCacheKey in cacheKey.CacheDependencies)
                {
                    if (string.IsNullOrWhiteSpace(rootCacheKey))
                    {
                        continue;
                    }

                    clearDependencyValues(rootCacheKey);
                    _dependenciesCacheManager.Remove(_keyPrefix + rootCacheKey);
                }
            });
        }

        private void clearDependencyValues(string rootCacheKey)
        {
            var dependencyKeys = _dependenciesCacheManager.Get(_keyPrefix + rootCacheKey);
            if (dependencyKeys == null)
            {
                return;
            }

            foreach (var dependencyKey in dependencyKeys)
            {
                _valuesCacheManager.Remove(_keyPrefix + dependencyKey);
            }
        }
    }
}