using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hian.NetworkUtilities
{
    /// <summary>
    /// HttpClient의 래퍼 클래스
    /// </summary>
    public class HttpClientWrapper : IHttpClient, IDisposable
    {
        private static readonly HttpClient _defaultClient = new HttpClient();
        private readonly HttpClient _client;
        private readonly bool _ownsClient;
        private readonly HttpClientSettings _settings;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly SemaphoreSlim _semaphore;
        private readonly List<TimeSpan> _responseTimes = new();
        private long _totalRequests;
        private long _failedRequests;
        private bool _disposed;

        public TimeSpan Timeout => _client.Timeout;
        public Uri BaseAddress => _client.BaseAddress;
        public TimeSpan AverageResponseTime
        {
            get
            {
                lock (_responseTimes)
                {
                    return _responseTimes.Count > 0 
                        ? TimeSpan.FromTicks((long)_responseTimes.Average(t => t.Ticks)) 
                        : TimeSpan.Zero;
                }
            }
        }
        public int TotalRequestCount => (int)_totalRequests;
        public int FailedRequestCount => (int)_failedRequests;
        public bool IsReady => !_disposed && !_cts.IsCancellationRequested;

        /// <summary>
        /// HttpClientWrapper의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="settings">HTTP 클라이언트 설정</param>
        /// <param name="client">사용자 정의 HttpClient 인스턴스</param>
        public HttpClientWrapper(HttpClientSettings settings = null, HttpClient client = null)
        {
            _disposed = false;
            _settings = settings ?? HttpClientSettings.Default;
            _settings.Validate();

            var handler = _settings.Handler ?? new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 50,
                UseCookies = true
            };

            _client = client ?? new HttpClient(handler);
            _ownsClient = client == null;
            _semaphore = new SemaphoreSlim(_settings.MaxConcurrentRequests);

            ConfigureClient();
        }

        private void ConfigureClient()
        {
            if (_client != _defaultClient)
            {
                _client.Timeout = _settings.Timeout;
                _client.BaseAddress = _settings.BaseAddress;
            }
        }

        private void TrackRequestMetrics(TimeSpan duration, bool isSuccess)
        {
            Interlocked.Increment(ref _totalRequests);
            if (!isSuccess)
                Interlocked.Increment(ref _failedRequests);
            lock (_responseTimes)
            {
                _responseTimes.Add(duration);
                if (_responseTimes.Count > _settings.MaxMetricsCount)
                    _responseTimes.RemoveAt(0);
            }
        }

        public void AddDefaultHeader(string name, string value)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            _client.DefaultRequestHeaders.Add(name, value);
        }

        public void RemoveDefaultHeader(string name)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            _client.DefaultRequestHeaders.Remove(name);
        }

        public async Task WaitForPendingRequestsAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await _semaphore.WaitAsync(cancellationToken);
            _semaphore.Release();
        }

        public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            var startTime = DateTime.UtcNow;
            bool success = false;
            try
            {
                var result = await ExecuteWithRetryAndSemaphoreAsync(async () => 
                {
                    try
                    {
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
                        var response = await _client.GetAsync(url, linkedCts.Token);
                        var content = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException(
                                $"Request failed: {response.StatusCode} - {content}");
                        }

                        return content;
                    }
                    catch (TaskCanceledException)
                    {
                        throw new HttpRequestException("Request was cancelled");
                    }
                    catch (TimeoutException)
                    {
                        throw new HttpRequestException("Request timed out");
                    }
                    catch (HttpRequestException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new HttpRequestException("Request failed", ex);
                    }
                }, cancellationToken);
                success = true;
                return result;
            }
            finally
            {
                TrackRequestMetrics(DateTime.UtcNow - startTime, success);
            }
        }

        public async Task<string> PostAsync(string url, string content, string contentType = "application/json", CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            var startTime = DateTime.UtcNow;
            bool success = false;
            try
            {
                var result = await ExecuteWithRetryAndSemaphoreAsync(async () =>
                {
                    try
                    {
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
                        var stringContent = new StringContent(content, Encoding.UTF8, contentType);
                        var response = await _client.PostAsync(url, stringContent, linkedCts.Token);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException(
                                $"Request failed: {response.StatusCode} - {responseContent}");
                        }

                        return responseContent;
                    }
                    catch (TaskCanceledException)
                    {
                        throw new HttpRequestException("Request was cancelled");
                    }
                    catch (TimeoutException)
                    {
                        throw new HttpRequestException("Request timed out");
                    }
                    catch (HttpRequestException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new HttpRequestException("Request failed", ex);
                    }
                }, cancellationToken);
                success = true;
                return result;
            }
            finally
            {
                TrackRequestMetrics(DateTime.UtcNow - startTime, success);
            }
        }

        public async Task<string> PutAsync(string url, string content, string contentType = "application/json", CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            var startTime = DateTime.UtcNow;
            bool success = false;
            try
            {
                var result = await ExecuteWithRetryAndSemaphoreAsync(async () =>
                {
                    try
                    {
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
                        var stringContent = new StringContent(content, Encoding.UTF8, contentType);
                        var response = await _client.PutAsync(url, stringContent, linkedCts.Token);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException(
                                $"Request failed: {response.StatusCode} - {responseContent}");
                        }

                        return responseContent;
                    }
                    catch (TaskCanceledException)
                    {
                        throw new HttpRequestException("Request was cancelled");
                    }
                    catch (TimeoutException)
                    {
                        throw new HttpRequestException("Request timed out");
                    }
                    catch (HttpRequestException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new HttpRequestException("Request failed", ex);
                    }
                }, cancellationToken);
                success = true;
                return result;
            }
            finally
            {
                TrackRequestMetrics(DateTime.UtcNow - startTime, success);
            }
        }

        public async Task<string> DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            var startTime = DateTime.UtcNow;
            bool success = false;
            try
            {
                var result = await ExecuteWithRetryAndSemaphoreAsync(async () =>
                {
                    try
                    {
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
                        var response = await _client.DeleteAsync(url, linkedCts.Token);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException(
                                $"Request failed: {response.StatusCode} - {responseContent}");
                        }

                        return responseContent;
                    }
                    catch (TaskCanceledException)
                    {
                        throw new HttpRequestException("Request was cancelled");
                    }
                    catch (TimeoutException)
                    {
                        throw new HttpRequestException("Request timed out");
                    }
                    catch (HttpRequestException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new HttpRequestException("Request failed", ex);
                    }
                }, cancellationToken);
                success = true;
                return result;
            }
            finally
            {
                TrackRequestMetrics(DateTime.UtcNow - startTime, success);
            }
        }

        
        public async Task<string> PatchAsync(string url, string content, string contentType = "application/json", CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            var startTime = DateTime.UtcNow;
            bool success = false;
            try
            {
                var result = await ExecuteWithRetryAndSemaphoreAsync(async () =>
                {
                    try
                    {
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
                        using var stringContent = new StringContent(content, Encoding.UTF8, contentType);
                        using var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = stringContent };
                        var response = await _client.SendAsync(request, linkedCts.Token);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException(
                                $"Request failed: {response.StatusCode} - {responseContent}");
                        }

                        return responseContent;
                    }
                    catch (TaskCanceledException)
                    {
                        throw new HttpRequestException("Request was cancelled");
                    }
                    catch (TimeoutException)
                    {
                        throw new HttpRequestException("Request timed out");
                    }
                    catch (HttpRequestException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new HttpRequestException("Request failed", ex);
                    }
                }, cancellationToken);
                success = true;
                return result;
            }
            finally
            {
                TrackRequestMetrics(DateTime.UtcNow - startTime, success);
            }
        }

        public async Task<HttpResponseMessage> HeadAsync(string url, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            var startTime = DateTime.UtcNow;
            bool success = false;
            try
            {
                var result = await ExecuteWithRetryAndSemaphoreAsync(async () =>
                {
                    try
                    {
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
                        var request = new HttpRequestMessage(HttpMethod.Head, url);
                        var response = await _client.SendAsync(request, linkedCts.Token);

                        if (!response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            throw new HttpRequestException(
                                $"Request failed: {response.StatusCode} - {content}");
                        }

                        return response;
                    }
                    catch (TaskCanceledException)
                    {
                        throw new HttpRequestException("Request was cancelled");
                    }
                    catch (TimeoutException)
                    {
                        throw new HttpRequestException("Request timed out");
                    }
                    catch (HttpRequestException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new HttpRequestException("Request failed", ex);
                    }
                }, cancellationToken);
                success = true;
                return result;
            }
            finally
            {
                TrackRequestMetrics(DateTime.UtcNow - startTime, success);
            }
        }

        public void SetTimeout(int milliseconds)
        {
            ThrowIfDisposed();
            if (milliseconds <= 0)
                throw new ArgumentException("Timeout must be greater than 0", nameof(milliseconds));
            
            if (_client != _defaultClient)
            {
                _client.Timeout = TimeSpan.FromMilliseconds(milliseconds);
            }
        }

        public void CancelPendingRequests()
        {
            ThrowIfDisposed();
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // 이미 Dispose된 경우 무시
            }
        }

        // 재시도 로직을 포함한 요청 메서드
        private TimeSpan CalculateRetryDelay(int retryCount)
        {
            const int maxDelayMinutes = 2;
            var delay = _settings.RetryDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1);
            return TimeSpan.FromMilliseconds(Math.Min(delay, TimeSpan.FromMinutes(maxDelayMinutes).TotalMilliseconds));
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            int retryCount = 0;
            Exception lastException = null;

            while (retryCount <= _settings.RetryCount)
            {
                try
                {
                    var result = await action();
                    return result;
                }
                catch (HttpRequestException httpEx)
                {
                    // RetryableStatusCodes 설정을 활용하도록 수정
                    var statusCode = GetStatusCodeFromException(httpEx);
                    if (statusCode.HasValue && _settings.RetryableStatusCodes.Contains(statusCode.Value))
                    {
                        lastException = httpEx;
                        retryCount++;

                        if (retryCount <= _settings.RetryCount)
                        {
                            Debug.LogWarning($"Request failed, retrying ({retryCount}/{_settings.RetryCount}): {httpEx.Message}");
                            var delay = CalculateRetryDelay(retryCount);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }
                    }

                    Debug.LogWarning($"Request failed with non-retryable status code {statusCode}: {httpEx.Message}");
                    throw;
                }
            }

            throw lastException ?? new HttpRequestException("Maximum retry attempts reached");
        }

        private HttpStatusCode? GetStatusCodeFromException(HttpRequestException ex)
        {
            // InternalServerError 문자열을 체크
            if (ex.Message.Contains("InternalServerError"))
                return HttpStatusCode.InternalServerError;
            
            // 숫자로 된 상태 코드를 찾아서 파싱
            var statusCodeMatch = Regex.Match(ex.Message, @"\b(\d{3})\b");
            if (statusCodeMatch.Success && int.TryParse(statusCodeMatch.Groups[1].Value, out int statusCode))
            {
                return (HttpStatusCode)statusCode;
            }
            
            return null;
        }

        private async Task<T> ExecuteWithRetryAndSemaphoreAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            await _semaphore.WaitAsync(linkedCts.Token);
            try
            {
                return await ExecuteWithRetryAsync(action, linkedCts.Token);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            
            if (disposing)
            {
                CancelPendingRequests();
                _cts.Dispose();
                _semaphore.Dispose();
                if (_ownsClient) _client.Dispose();
            }
            
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // 모든 public 메서드에 disposed 체크 추가
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HttpClientWrapper));
        }

        ~HttpClientWrapper()
        {
            Dispose(false);
        }
    }
} 