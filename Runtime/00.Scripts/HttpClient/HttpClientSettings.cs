using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;

namespace Hian.NetworkUtilities
{
    /// <summary>
    /// HTTP 클라이언트 설정
    /// </summary>
    public class HttpClientSettings
    {
        private int _retryCount = 3;
        private TimeSpan _retryDelay = TimeSpan.FromSeconds(1);
        private TimeSpan _timeout = TimeSpan.FromSeconds(100);
        private int _maxConcurrentRequests = 10;
        private int _maxMetricsCount = 1000;
        private HashSet<HttpStatusCode> _retryableStatusCodes = new();

        /// <summary>
        /// HTTP 핸들러 설정
        /// </summary>
        public HttpClientHandler Handler { get; set; }

        /// <summary>
        /// 요청 타임아웃
        /// </summary>
        public TimeSpan Timeout 
        { 
            get => _timeout;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentException("Timeout must be greater than zero", nameof(value));
                _timeout = value;
            }
        }

        /// <summary>
        /// 기본 주소
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// HTTP 버전
        /// </summary>
        public Version HttpVersion { get; set; }

        /// <summary>
        /// 재시도 횟수
        /// </summary>
        public int RetryCount 
        { 
            get => _retryCount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Retry count cannot be negative", nameof(value));
                _retryCount = value;
            }
        }

        /// <summary>
        /// 재시도 대기 시간
        /// </summary>
        public TimeSpan RetryDelay 
        { 
            get => _retryDelay;
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentException("Retry delay cannot be negative", nameof(value));
                _retryDelay = value;
            }
        }

        /// <summary>
        /// 동시 요청 제한 수
        /// </summary>
        public int MaxConcurrentRequests 
        { 
            get => _maxConcurrentRequests;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Max concurrent requests must be greater than zero", nameof(value));
                _maxConcurrentRequests = value;
            }
        }

        /// <summary>
        /// 메트릭스 수집 최대 개수
        /// </summary>
        public int MaxMetricsCount 
        { 
            get => _maxMetricsCount;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Max metrics count must be greater than zero", nameof(value));
                _maxMetricsCount = value;
            }
        }

        /// <summary>
        /// 요청 실패 시 재시도할 HTTP 상태 코드들
        /// </summary>
        public HashSet<HttpStatusCode> RetryableStatusCodes 
        { 
            get => _retryableStatusCodes;
            set => _retryableStatusCodes = value ?? new HashSet<HttpStatusCode>();
        }

        /// <summary>
        /// 기본 설정을 반환합니다.
        /// </summary>
        public static HttpClientSettings Default
        {
            get
            {
                var settings = new HttpClientSettings
                {
                    Handler = new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        AllowAutoRedirect = true,
                        MaxAutomaticRedirections = 20,
                        UseCookies = true
                    },
                    RetryableStatusCodes = new HashSet<HttpStatusCode>
                    {
                        HttpStatusCode.RequestTimeout,
                        HttpStatusCode.InternalServerError,
                        HttpStatusCode.BadGateway,
                        HttpStatusCode.ServiceUnavailable,
                        HttpStatusCode.GatewayTimeout
                    }
                };
                settings.Validate();
                return settings;
            }
        }

        /// <summary>
        /// 설정의 유효성을 검사합니다.
        /// </summary>
        public void Validate()
        {
            if (Timeout <= TimeSpan.Zero)
                throw new ArgumentException("Timeout must be greater than zero");
            
            if (RetryCount < 0)
                throw new ArgumentException("Retry count cannot be negative");
            
            if (RetryDelay <= TimeSpan.Zero)
                throw new ArgumentException("Retry delay must be greater than zero");
            
            if (MaxConcurrentRequests <= 0)
                throw new ArgumentException("Max concurrent requests must be greater than zero");

            if (MaxMetricsCount <= 0)
                throw new ArgumentException("Max metrics count must be greater than zero");
        }

        /// <summary>
        /// 현재 설정의 복사본을 생성합니다.
        /// </summary>
        public HttpClientSettings Clone()
        {
            var clone = new HttpClientSettings
            {
                Handler = Handler != null ? new HttpClientHandler
                {
                    AutomaticDecompression = Handler.AutomaticDecompression,
                    AllowAutoRedirect = Handler.AllowAutoRedirect,
                    MaxAutomaticRedirections = Handler.MaxAutomaticRedirections,
                    UseCookies = Handler.UseCookies
                } : null,
                Timeout = Timeout,
                BaseAddress = BaseAddress,
                HttpVersion = HttpVersion,
                RetryCount = RetryCount,
                RetryDelay = RetryDelay,
                MaxConcurrentRequests = MaxConcurrentRequests,
                MaxMetricsCount = MaxMetricsCount,
                RetryableStatusCodes = new HashSet<HttpStatusCode>(_retryableStatusCodes)
            };
            return clone;
        }

        /// <summary>
        /// 설정을 수정하고 유효성을 검사한 후 복사본을 반환합니다.
        /// </summary>
        public HttpClientSettings With(Action<HttpClientSettings> modifier)
        {
            var clone = this.Clone();
            modifier(clone);
            clone.Validate();
            return clone;
        }
    }
} 