using System;

namespace Hian.NetworkUtilities
{
    /// <summary>
    /// 캐시 항목을 나타내는 내부 클래스입니다.
    /// </summary>
    internal class CacheItem
    {
        /// <summary>
        /// 캐시된 데이터입니다.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 캐시 항목의 만료 시간입니다.
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// 캐시 항목이 생성된 시간입니다.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
