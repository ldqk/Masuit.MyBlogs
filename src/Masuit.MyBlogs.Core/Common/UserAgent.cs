using Microsoft.Extensions.Caching.Memory;
using System;

namespace Masuit.MyBlogs.Core.Common
{
    public class UserAgent
    {
        private static readonly MemoryCacheOptions CacheOptions = new MemoryCacheOptions();
        private static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        public UserAgent()
        {
            CacheOptions.SizeLimit = 10000;
        }

        public static UA Parse(string userAgentString)
        {
            userAgentString = userAgentString.Length > 512 ? userAgentString.Trim().Substring(0, 512) : userAgentString.Trim();
            return Cache.GetOrCreate(userAgentString, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(3);
                entry.Size = 1;
                return new UA(userAgentString);
            });
        }

        public UA ParseUserAgent(string userAgentString)
        {
            userAgentString = userAgentString.Length > 512 ? userAgentString.Trim().Substring(0, 512) : userAgentString.Trim();
            return Cache.GetOrCreate(userAgentString, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(3);
                entry.Size = 1;
                return new UA(userAgentString);
            });
        }

    }
}