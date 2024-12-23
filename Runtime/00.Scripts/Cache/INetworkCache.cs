using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hian.NetworkUtilities
{
    /// <summary>
    /// 네트워크 캐시 관리를 위한 인터페이스입니다.
    /// </summary>
    public interface INetworkCache : IDisposable
    {
        #region Events
        event Action<string> OnCacheRemoved;
        event Action<string> OnCacheExpired;
        event Action<string> OnCacheAdded;
        #endregion

        #region Synchronous Methods
        bool TryGetCache<T>(string key, out T data);
        void SetCache(string key, object data, float expirationMinutes = 30);
        bool RemoveCache(string key);
        void ClearCache();
        bool UpdateExpirationTime(string key, float expirationMinutes);
        #endregion

        #region Asynchronous Methods
        Task<(bool success, T data)> TryGetCacheAsync<T>(string key);
        Task SetCacheAsync(
            string key,
            object data,
            float expirationMinutes = 30,
            CancellationToken cancellationToken = default
        );
        Task RemoveCacheAsync(string key, CancellationToken cancellationToken = default);
        Task ClearCacheAsync(CancellationToken cancellationToken = default);
        Task UpdateExpirationTimeAsync(
            string key,
            float expirationMinutes,
            CancellationToken cancellationToken = default
        );
        #endregion

        #region Cache Management
        void CleanExpiredCache(int batchSize = 100);
        Task CleanAllExpiredCacheAsync(
            int batchSize = 100,
            CancellationToken cancellationToken = default
        );
        void PauseCleanup();
        void ResumeCleanup(int intervalMinutes = 5);
        CacheStats GetStats();
        void UnsubscribeAll();
        #endregion
    }

    /// <summary>
    /// 캐시 구현을 위한 추가 인터페이스입니다.
    /// </summary>
    public interface INetworkCacheProvider
    {
        INetworkCache CreateCache(int maxCacheSize = 1000, int cleanupIntervalMinutes = 5);
    }
}
