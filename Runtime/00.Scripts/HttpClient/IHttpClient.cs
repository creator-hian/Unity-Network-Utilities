using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace Hian.NetworkUtilities
{
    /// <summary>
    /// HTTP 클라이언트 인터페이스
    /// </summary>
    public interface IHttpClient : IDisposable
    {
        /// <summary>
        /// 현재 설정된 타임아웃 값을 가져옵니다.
        /// </summary>
        TimeSpan Timeout { get; }

        /// <summary>
        /// 기본 주소를 가져옵니다.
        /// </summary>
        Uri BaseAddress { get; }

        /// <summary>
        /// GET 요청을 비동기적으로 수행합니다.
        /// </summary>
        Task<string> GetAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST 요청을 비동기적으로 수행합니다.
        /// </summary>
        Task<string> PostAsync(string url, string content, string contentType = "application/json", CancellationToken cancellationToken = default);

        /// <summary>
        /// PUT 요청을 비동기적으로 수행합니다.
        /// </summary>
        Task<string> PutAsync(string url, string content, string contentType = "application/json", CancellationToken cancellationToken = default);

        /// <summary>
        /// DELETE 요청을 비동기적으로 수행합니다.
        /// </summary>
        Task<string> DeleteAsync(string url, CancellationToken cancellationToken = default);


        /// <summary>
        /// PATCH 요청을 비동기적으로 수행합니다.
        /// </summary>
        Task<string> PatchAsync(string url, string content, string contentType = "application/json", CancellationToken cancellationToken = default);

        /// <summary>
        /// HEAD 요청을 비동기적으로 수행합니다.
        /// </summary>
        Task<HttpResponseMessage> HeadAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// 타임아웃을 설정합니다.
        /// </summary>
        void SetTimeout(int milliseconds);

        void CancelPendingRequests();

        /// <summary>
        /// 요청 헤더 관리 추가
        /// </summary>
        void AddDefaultHeader(string name, string value);
        void RemoveDefaultHeader(string name);

        /// <summary>
        /// 진단 정보 추가
        /// </summary>
        TimeSpan AverageResponseTime { get; }
        int TotalRequestCount { get; }
        int FailedRequestCount { get; }
        bool IsReady { get; }

        Task WaitForPendingRequestsAsync(CancellationToken cancellationToken = default);

    }
} 