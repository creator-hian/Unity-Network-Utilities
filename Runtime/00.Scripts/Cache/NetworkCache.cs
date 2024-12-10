using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hian.NetworkUtilities
{
    /// <summary>
    /// 네트워크 요청 결과를 캐싱하는 스레드 안전한 캐시 관리자입니다.
    /// </summary>
    /// <remarks>
    /// LRU(Least Recently Used) 캐시 교체 정책을 사용하며,
    /// 동기 및 비동기 작업을 모두 지원합니다.
    /// </remarks>
    public class NetworkCache : INetworkCache
    {
        #region Fields

        private readonly ConcurrentDictionary<string, CacheItem> _cache;
        private readonly LinkedList<string> _lruList;
        private readonly object _lruLock = new object();
        private readonly int _maxCacheSize;
        private readonly Timer _cleanupTimer;
        private volatile bool _disposed;

        /// <summary>
        /// 캐시 항목이 제거될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<string> OnCacheRemoved;

        /// <summary>
        /// 캐시 항목이 만료될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<string> OnCacheExpired;

        /// <summary>
        /// 새로운 캐시 항목이 추가될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<string> OnCacheAdded;

        #endregion

        #region Constructor

        /// <summary>
        /// NetworkCache의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="maxCacheSize">최대 캐시 항목 수</param>
        /// <param name="cleanupIntervalMinutes">캐시 정리 간격(분)</param>
        public NetworkCache(int maxCacheSize = 1000, int cleanupIntervalMinutes = 5)
        {
            _maxCacheSize = maxCacheSize;
            _cache = new ConcurrentDictionary<string, CacheItem>();
            _lruList = new LinkedList<string>();
            
            _cleanupTimer = new Timer(CleanupCallback, null, 
                TimeSpan.FromMinutes(cleanupIntervalMinutes), 
                TimeSpan.FromMinutes(cleanupIntervalMinutes));
        }

        #endregion

        #region Internal Helper Methods

        private void CleanupCallback(object state)
        {
            try
            {
                CleanExpiredCache();
            }
            catch (Exception ex)
            {
                    // TODO: Unity Main Thread에서의 처리가 필요하며, 이에 따라 Package Dependency를 추가할지, 어떻게 처리할지 확인이 필요함.
                Debug.LogError($"Cache cleanup failed: {ex.Message}");
            }
        }

        private (bool success, CacheItem item) GetValidCacheItem(string key)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpirationTime)
                {
                    return (true, item);
                }
                RemoveCache(key);
                InvokeEvent(OnCacheExpired, key);
            }
            return (false, null);
        }

        private void InvokeEvent(Action<string> eventHandler, string key)
        {
            if (eventHandler == null) return;

            foreach (var handler in eventHandler.GetInvocationList())
            {
                try
                {
                    ((Action<string>)handler)(key);
                }
                catch (Exception ex)
                {
                    // TODO: Unity Main Thread에서의 처리가 필요하며, 이에 따라 Package Dependency를 추가할지, 어떻게 처리할지 확인이 필요함.
                    Debug.LogError($"Cache event handler error for {key}: {ex.Message}");
                }
            }
        }

        private (bool success, T data) TryGetCacheInternal<T>(string key)
        {
            var (success, item) = GetValidCacheItem(key);
            if (success)
            {
                try
                {
                    var data = (T)item.Data;
                    UpdateLRU(key);
                    return (true, data);
                }
                catch (InvalidCastException)
                {
                    RemoveCache(key);
                }
            }
            return (false, default);
        }

        private void AddOrUpdateCacheItem(string key, CacheItem item)
        {
            const int maxRetries = 3;
            var retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                var currentCount = _cache.Count;
                if (currentCount < _maxCacheSize || _cache.ContainsKey(key))
                {
                    _cache.AddOrUpdate(key, item, (_, _) => item);
                    UpdateLRU(key);
                    InvokeEvent(OnCacheAdded, key);
                    return;
                }
                
                RemoveOldestItem();
                retryCount++;
            }
            
            throw new InvalidOperationException("Failed to add cache item after maximum retries");
        }

        private void UpdateLRU(string key)
        {
            lock (_lruLock)
            {
                var node = _lruList.Find(key);
                if (node != null)
                {
                    _lruList.Remove(node);
                }
                _lruList.AddFirst(key);
            }
        }

        private void RemoveOldestItem()
        {
            lock (_lruLock)
            {
                if (_lruList.Last != null)
                {
                    var oldestKey = _lruList.Last.Value;
                    _lruList.RemoveLast();
                    if (_cache.TryRemove(oldestKey, out _))
                    {
                        InvokeEvent(OnCacheRemoved, oldestKey);
                    }
                }
            }
        }

        #endregion

        #region Public Synchronous Methods

        /// <summary>
        /// 지정된 키에 해당하는 캐시 데이터를 조회합니다.
        /// </summary>
        /// <typeparam name="T">반환할 데이터 타입</typeparam>
        /// <param name="key">캐시 키</param>
        /// <param name="data">조회된 데이터</param>
        /// <returns>데이터 존재 여부</returns>
        public bool TryGetCache<T>(string key, out T data)
        {
            ThrowIfDisposed();
            var result = TryGetCacheInternal<T>(key);
            data = result.data;
            return result.success;
        }

        /// <summary>
        /// 캐시에 데이터를 저장합니다.
        /// </summary>
        /// <param name="key">캐시 키</param>
        /// <param name="data">저장할 데이터</param>
        /// <param name="expirationMinutes">만료 시간(분)</param>
        public void SetCache(string key, object data, float expirationMinutes = 30)
        {
            ThrowIfDisposed();
            var now = DateTime.UtcNow;
            var cacheItem = new CacheItem
            {
                Data = data,
                ExpirationTime = now.AddMinutes(expirationMinutes),
                CreationTime = now
            };

            AddOrUpdateCacheItem(key, cacheItem);
        }

        /// <summary>
        /// 지정된 키의 캐시를 제거합니다.
        /// </summary>
        /// <param name="key">제거할 캐시의 키</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveCache(string key)
        {
            ThrowIfDisposed();
            if (_cache.TryRemove(key, out _))
            {
                lock (_lruLock)
                {
                    _lruList.Remove(key);
                }
                InvokeEvent(OnCacheRemoved, key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 모든 캐시를 삭제합니다.
        /// </summary>
        public void ClearCache()
        {
            ThrowIfDisposed();
            var keys = _cache.Keys.ToList();
            _cache.Clear();
            lock (_lruLock)
            {
                _lruList.Clear();
            }
            foreach (var key in keys)
            {
                InvokeEvent(OnCacheRemoved, key);
            }
        }

        /// <summary>
        /// 지정된 키의 캐시 만료 시간을 업데이트합니다.
        /// </summary>
        /// <param name="key">캐시 키</param>
        /// <param name="expirationMinutes">새로운 만료 시간(분)</param>
        /// <returns>업데이트 성공 여부</returns>
        public bool UpdateExpirationTime(string key, float expirationMinutes)
        {
            ThrowIfDisposed();
            if (_cache.TryGetValue(key, out var item))
            {
                var now = DateTime.UtcNow;
                item.ExpirationTime = now.AddMinutes(expirationMinutes);
                return _cache.TryUpdate(key, item, item);
            }
            return false;
        }

        #endregion

        #region Public Asynchronous Methods

        /// <summary>
        /// 지정된 키에 해당하는 캐시 데이터를 비동기적으로 조회합니다.
        /// </summary>
        public Task<(bool success, T data)> TryGetCacheAsync<T>(string key)
        {
            ThrowIfDisposed();
            return Task.FromResult(TryGetCacheInternal<T>(key));
        }

        /// <summary>
        /// 캐시에 데이터를 비동기적으로 저장합니다.
        /// </summary>
        public async Task SetCacheAsync(string key, object data, float expirationMinutes = 30, 
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            try
            {
                var now = DateTime.UtcNow;
                var cacheItem = new CacheItem
                {
                    Data = data,
                    ExpirationTime = now.AddMinutes(expirationMinutes),
                    CreationTime = now
                };

                await Task.Run(() => 
                {
                    linkedCts.Token.ThrowIfCancellationRequested();
                    AddOrUpdateCacheItem(key, cacheItem);
                }, linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                // 작업 취소 처리
                throw;
            }
        }

        /// <summary>
        /// 지정된 키의 캐시를 비동기적으로 제거합니다.
        /// </summary>
        public async Task RemoveCacheAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_cache.TryRemove(key, out _))
                {
                    lock (_lruLock)
                    {
                        _lruList.Remove(key);
                    }
                    InvokeEvent(OnCacheRemoved, key);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 모든 캐시를 비동기적으로 삭제합니다.
        /// </summary>
        public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var keys = _cache.Keys.ToList();
                _cache.Clear();
                lock (_lruLock)
                {
                    _lruList.Clear();
                }
                foreach (var key in keys)
                {
                    InvokeEvent(OnCacheRemoved, key);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 지정된 키의 캐시 만료 시간을 비동기적으로 업데이트합니다.
        /// </summary>
        public async Task UpdateExpirationTimeAsync(string key, float expirationMinutes, 
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_cache.TryGetValue(key, out var item))
                {
                    var now = DateTime.UtcNow;
                    item.ExpirationTime = now.AddMinutes(expirationMinutes);
                    _cache.TryUpdate(key, item, item);
                }
            }, cancellationToken);
        }

        #endregion

        #region Cache Management Methods

        /// <summary>
        /// 만료된 캐시 항목들을 정리합니다.
        /// </summary>
        /// <param name="batchSize">한 번에 처리할 최대 항목 수</param>
        public void CleanExpiredCache(int batchSize = 100)
        {
            ThrowIfDisposed();
            var now = DateTime.UtcNow;
            var count = 0;
            
            foreach (var kvp in _cache)
            {
                if (count >= batchSize) break;
                
                if (now >= kvp.Value.ExpirationTime)
                {
                    if (_cache.TryRemove(kvp.Key, out _))
                    {
                        count++;
                        lock (_lruLock)
                        {
                            _lruList.Remove(kvp.Key);
                        }
                        InvokeEvent(OnCacheExpired, kvp.Key);
                    }
                }
            }
        }

        /// <summary>
        /// 모든 만료된 캐시를 배치 처리 방식으로 정리합니다.
        /// </summary>
        public async Task CleanAllExpiredCacheAsync(int batchSize = 100, 
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            while (!cancellationToken.IsCancellationRequested)
            {
                var initialCount = _cache.Count;
                CleanExpiredCache(batchSize);
                
                if (_cache.Count == initialCount)
                    break;
                    
                await Task.Yield();
            }
        }

        /// <summary>
        /// 캐시 정리 타이머를 일시 중지합니다.
        /// </summary>
        public void PauseCleanup()
        {
            ThrowIfDisposed();
            _cleanupTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 캐시 정리 타이머를 재개합니다.
        /// </summary>
        /// <param name="intervalMinutes">정리 간격(분)</param>
        public void ResumeCleanup(int intervalMinutes = 5)
        {
            ThrowIfDisposed();
            _cleanupTimer?.Change(
                TimeSpan.FromMinutes(intervalMinutes),
                TimeSpan.FromMinutes(intervalMinutes));
        }

        #endregion

        #region Event Management

        /// <summary>
        /// 모든 이벤트 구독을 해제합니다.
        /// </summary>
        public void UnsubscribeAll()
        {
            OnCacheRemoved = null;
            OnCacheExpired = null;
            OnCacheAdded = null;
        }

        #endregion

        #region Resource Management

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(NetworkCache));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cleanupTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    _cleanupTimer?.Dispose();
                    UnsubscribeAll();
                    _cache?.Clear();
                    _lruList?.Clear();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public CacheStats GetStats()
        {
            return CacheStats.Create(_cache, _maxCacheSize);
        }

        ~NetworkCache()
        {
            Dispose(false);
        }

        #endregion

    }

    /// <summary>
    /// 기본 NetworkCache 구현을 제공하는 Provider입니다.
    /// </summary>
    public class DefaultNetworkCacheProvider : INetworkCacheProvider
    {
        public INetworkCache CreateCache(int maxCacheSize = 1000, int cleanupIntervalMinutes = 5)
        {
            return new NetworkCache(maxCacheSize, cleanupIntervalMinutes);
        }
    }
} 