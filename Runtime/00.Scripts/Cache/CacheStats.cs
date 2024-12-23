using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Hian.NetworkUtilities
{
    /// <summary>
    /// 캐시 통계 정보를 나타내는 구조체입니다.
    /// </summary>
    public struct CacheStats
    {
        /// <summary>
        /// 전체 캐시 항목 수입니다.
        /// </summary>
        public int TotalItems { get; }

        /// <summary>
        /// 최대 캐시 크기입니다.
        /// </summary>
        public int MaxCacheSize { get; }

        /// <summary>
        /// 활성 상태인 캐시 항목 수입니다.
        /// </summary>
        public int ActiveItems { get; }

        /// <summary>
        /// 만료된 캐시 항목 수입니다.
        /// </summary>
        public int ExpiredItems { get; }

        private CacheStats(ConcurrentDictionary<string, CacheItem> cache, int maxCacheSize)
        {
            var now = DateTime.UtcNow;
            TotalItems = cache.Count;
            MaxCacheSize = maxCacheSize;
            ActiveItems = cache.Count(kvp => now < kvp.Value.ExpirationTime);
            ExpiredItems = cache.Count(kvp => now >= kvp.Value.ExpirationTime);
        }

        internal static CacheStats Create(
            ConcurrentDictionary<string, CacheItem> cache,
            int maxCacheSize
        )
        {
            return new CacheStats(cache, maxCacheSize);
        }
    }
}
