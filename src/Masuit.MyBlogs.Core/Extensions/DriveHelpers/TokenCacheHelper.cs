using Microsoft.Identity.Client;

namespace Masuit.MyBlogs.Core.Extensions.DriveHelpers
{
    /// <summary>
    /// 缓存 Token
    /// </summary>
    static class TokenCacheHelper
    {
        /// <summary>
        /// Path to the token cache
        /// </summary>
        public static readonly string CacheFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "TokenCache.bin");

        private static readonly object FileLock = new();
        public static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(args =>
            {
                lock (FileLock)
                {
                    args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath) ? File.ReadAllBytes(CacheFilePath) : null);
                }
            });
            tokenCache.SetAfterAccess(args =>
            {
                if (args.HasStateChanged)
                {
                    lock (FileLock)
                    {
                        File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
                    }
                }
            });
        }

    }
}