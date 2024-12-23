using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using Ping = System.Net.NetworkInformation.Ping;

namespace Hian.NetworkUtilities
{
    public static class NetworkUtility
    {
        #region Constants
        private static readonly HttpClient httpClient = new HttpClient();

        // Timeout settings
        private const int DEFAULT_TIMEOUT = 5000;

        // Google DNS settings
        private const string GOOGLE_DNS = "8.8.8.8";

        // Microsoft NCSI settings
        private const string MSFT_TEST_URL = "http://www.msftncsi.com/ncsi.txt";
        private const string MSFT_TEST_RESULT = "Microsoft NCSI";
        private const string MSFT_DNS = "dns.msftncsi.com";
        private const string MSFT_DNS_IP = "131.107.255.255";

        // Public IP API
        private const string PUBLIC_IP_API = "https://api.ipify.org";

        private static event Action<bool> OnNetworkStatusChanged;
        private static bool _lastNetworkStatus;
        private static int _networkTimeout = DEFAULT_TIMEOUT;
        #endregion

        #region Network Status Check

        /// <summary>
        /// 인터넷 연결 상태를 동기적으로 확인합니다
        /// </summary>
        /// <returns>인터넷 연결이 가능한 경우 true, 그렇지 않은 경우 false</returns>
        /// <remarks>
        /// 이 메서드는 Google DNS와 Microsoft NCSI를 모두 확인하며,
        /// 둘 중 하나라도 성공하면 true를 반환합니다.
        /// </remarks>
        public static bool IsInternetAvailable()
        {
            return IsInternetAvailableByGoogle() || IsInternetAvailableByMicrosoft();
        }

        /// <summary>
        /// 인터넷 연결 상태를 비동기적으로 확인합니다
        /// </summary>
        /// <returns>인터넷 연결이 가능한 경우 true, 그렇지 않은 경우 false를 포함하는 Task</returns>
        /// <remarks>
        /// 이 메서드는 Google DNS와 Microsoft NCSI를 모두 확인하며,
        /// 둘 중 하나라도 성공하면 true를 반환합니다.
        /// </remarks>
        public static async Task<bool> IsInternetAvailableAsync()
        {
            return await IsInternetAvailableByGoogleAsync()
                || await IsInternetAvailableByMicrosoftAsync();
        }

        private static bool IsInternetAvailableByGoogle()
        {
            try
            {
                using Ping ping = new Ping();
                PingReply reply = ping.Send(GOOGLE_DNS, _networkTimeout);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Google DNS check failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> IsInternetAvailableByGoogleAsync()
        {
            try
            {
                using Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(GOOGLE_DNS, _networkTimeout);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Google DNS check failed: {ex.Message}");
                return false;
            }
        }

        private static bool IsInternetAvailableByMicrosoft()
        {
            try
            {
                string result = httpClient
                    .GetStringAsync(MSFT_TEST_URL)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                if (result != MSFT_TEST_RESULT)
                {
                    return false;
                }

                IPHostEntry dnsHost = Dns.GetHostEntry(MSFT_DNS);
                return dnsHost.AddressList.Length > 0
                    && dnsHost.AddressList[0].ToString() == MSFT_DNS_IP;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Microsoft NCSI check failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> IsInternetAvailableByMicrosoftAsync()
        {
            try
            {
                // NCSI 텍스트 파일 확인
                string result = await httpClient
                    .GetStringAsync(MSFT_TEST_URL)
                    .ConfigureAwait(false);
                if (result != MSFT_TEST_RESULT)
                {
                    return false;
                }

                // NCSI DNS 확인
                IPHostEntry dnsHost = await Dns.GetHostEntryAsync(MSFT_DNS).ConfigureAwait(false);
                return dnsHost.AddressList.Length > 0
                    && dnsHost.AddressList[0].ToString() == MSFT_DNS_IP;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Microsoft NCSI check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 네트워크 연결 상태를 확인합니다 (로컬 네트워크 포함)
        /// </summary>
        /// <returns>네트워크에 연결된 경우 true, 그렇지 않은 경우 false</returns>
        /// <remarks>
        /// 이 메서드는 인터넷 연결이 아닌 네트워크 어댑터의 연결 상태만을 확인합니다.
        /// 실제 인터넷 연결 확인을 위해서는 IsInternetAvailable 메서드를 사용하세요.
        /// </remarks>
        public static bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>
        /// 네트워크 상태 변경 이벤트
        /// </summary>
        /// <remarks>
        /// 네트워크 상태가 변경될 때 호출되며, 매개변수는 현재 네트워크 사용 가능 여부입니다.
        /// </remarks>
        public static event Action<bool> NetworkStatusChanged
        {
            add
            {
                OnNetworkStatusChanged += value;
                // 구독 시 현재 상태 즉시 전달
                value?.Invoke(IsNetworkAvailable());
            }
            remove { OnNetworkStatusChanged -= value; }
        }

        /// <summary>
        /// 로컬 IP 주소를 가져옵니다
        /// </summary>
        /// <returns>시스템의 주 네트워크 어댑터의 IPv4 주소</returns>
        /// <remarks>
        /// 활성화된 네트워크 어댑터 중 첫 번째 IPv4 주소를 반환합니다.
        /// 실패 시 빈 문자열을 반환합니다.
        /// </remarks>
        public static string GetLocalIPAddress()
        {
            try
            {
                return GetActiveNetworkInterfaces()
                        .SelectMany(static ni => ni.GetIPProperties().UnicastAddresses)
                        .Where(static ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        .Select(static ip => ip.Address.ToString())
                        .FirstOrDefault() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Failed to get local IP address: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 네트워크 상태 변경을 시뮬레이션합니다 (테스트 용도)
        /// </summary>
        /// <param name="isAvailable">시뮬레이션할 네트워크 상태</param>
        /// <remarks>
        /// 이 메서드는 테스트 목적으로만 사용됩며, 같은 어셈블리 내에서만 접근 가능합니다.
        /// </remarks>
        internal static void SimulateNetworkStatusChange(bool isAvailable)
        {
            if (_lastNetworkStatus != isAvailable)
            {
                _lastNetworkStatus = isAvailable;
                OnNetworkStatusChanged?.Invoke(isAvailable);
            }
        }

        /// <summary>
        /// 네트워크 작업의 타임아웃 값을 설정합니다.
        /// </summary>
        /// <param name="milliseconds">타임아웃 시간 (밀리초)</param>
        public static void SetNetworkTimeout(int milliseconds)
        {
            if (milliseconds <= 0)
            {
                throw new ArgumentException("Timeout must be greater than 0", nameof(milliseconds));
            }

            _networkTimeout = milliseconds;
        }

        #endregion

        #region IP Address Management

        /// <summary>
        /// 시스템의 모든 네트워크 IP 주소를 반환합니다
        /// </summary>
        /// <returns>활성화된 네트워크 인터페이스의 IPv4 주소 목록</returns>
        /// <remarks>
        /// 이 메서드는 시스템의 모든 활성 네트워크 어댑터에서 IPv4 주소만을 반환합니다.
        /// 실패 시 빈 목록을 반환합니다.
        /// </remarks>
        public static IList<string> GetAllNetworkIPs()
        {
            try
            {
                return GetActiveNetworkInterfaces()
                    .SelectMany(static ni => ni.GetIPProperties().UnicastAddresses)
                    .Where(static ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(static ip => ip.Address.ToString())
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Failed to get network IPs: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// 공인 IP 주소를 동기적으로 가져옵니다
        /// </summary>
        /// <returns>공인 IP 주소. 실패 시 빈 문자열 반환</returns>
        /// <remarks>
        /// 이 메서드는 네트워크 작업을 동기적으로 수행하므로,
        /// UI 스레드에서 호출 시 응답성에 영향을 줄 수 있습니다.
        /// </remarks>
        public static string GetPublicIP()
        {
            try
            {
                return httpClient
                    .GetStringAsync(PUBLIC_IP_API)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult()
                    .Trim();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Failed to get public IP: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 공인 IP 주소를 비동기적으로 가져옵니다
        /// </summary>
        /// <returns>공인 IP 주소를 포함한 Task. 실패 시 빈 문자열 반환</returns>
        public static async Task<string> GetPublicIPAsync()
        {
            try
            {
                string response = await httpClient
                    .GetStringAsync(PUBLIC_IP_API)
                    .ConfigureAwait(false);
                return response.Trim();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Failed to get public IP: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion

        #region Network Interface

        /// <summary>
        /// 활성화된 네트워크 인터페이스를 가져옵니다
        /// </summary>
        private static IEnumerable<NetworkInterface> GetActiveNetworkInterfaces()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(static ni => ni.OperationalStatus == OperationalStatus.Up);
        }

        /// <summary>
        /// 특정 네트워크 어댑터의 IP 주소를 가져옵니다
        /// </summary>
        /// <param name="adapterName">네트워크 어댑터의 이름</param>
        /// <returns>어댑터의 IPv4 주소. 실패 시 빈 문자열 반환</returns>
        /// <remarks>
        /// 지정된 이름의 어댑터가 없거나 비활성화된 경우 빈 문자열을 반환합니다.
        /// </remarks>
        public static string GetAdapterIPAddress(string adapterName)
        {
            if (string.IsNullOrEmpty(adapterName))
            {
                Debug.LogError("[NetworkUtility] Adapter name cannot be null or empty");
                return string.Empty;
            }

            try
            {
                NetworkInterface networkInterface = GetActiveNetworkInterfaces()
                    .FirstOrDefault(ni => ni.Name == adapterName);

                if (networkInterface == null)
                {
                    Debug.LogWarning($"[NetworkUtility] Network adapter '{adapterName}' not found");
                    return string.Empty;
                }

                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                if (ipProperties == null)
                {
                    Debug.LogError(
                        $"[NetworkUtility] Failed to get IP properties for adapter '{adapterName}'"
                    );
                    return string.Empty;
                }

                UnicastIPAddressInformation address = ipProperties.UnicastAddresses.FirstOrDefault(
                    ip => ip.Address.AddressFamily == AddressFamily.InterNetwork
                );

                if (address == null)
                {
                    Debug.LogWarning(
                        $"[NetworkUtility] No IPv4 address found for adapter '{adapterName}'"
                    );
                    return string.Empty;
                }

                return address.Address.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Failed to get adapter IP: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion

        #region Network Diagnostics

        /// <summary>
        /// 지정된 호스트에 대한 ping 테스트를 동기적으로 수행합니다
        /// </summary>
        /// <param name="host">ping을 수행할 호스트 주소</param>
        /// <param name="timeout">제한 시간 (밀리초)</param>
        /// <returns>ping 테스트 결과</returns>
        /// <exception cref="ArgumentNullException">host가 null인 경우</exception>
        /// <exception cref="ArgumentException">host 빈 문자열인 경우</exception>
        public static PingResult PingHost(string host, int timeout = DEFAULT_TIMEOUT)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host cannot be empty", nameof(host));
            }

            if (timeout <= 0)
            {
                throw new ArgumentException("Timeout must be greater than 0", nameof(timeout));
            }

            try
            {
                using Ping ping = new Ping();
                PingReply reply = ping.Send(host, timeout);
                return new PingResult
                {
                    IsSuccess = reply.Status == IPStatus.Success,
                    RoundtripTime = reply.RoundtripTime,
                    Status = reply.Status,
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Ping failed: {ex.Message}");
                return new PingResult { IsSuccess = false, Status = IPStatus.Unknown };
            }
        }

        /// <summary>
        /// 지정된 호스트에 대한 ping 테스트를 수행합니다
        /// </summary>
        public static async Task<PingResult> PingHostAsync(
            string host,
            int timeout = DEFAULT_TIMEOUT
        )
        {
            try
            {
                using Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(host, timeout);
                return new PingResult
                {
                    IsSuccess = reply.Status == IPStatus.Success,
                    RoundtripTime = reply.RoundtripTime,
                    Status = reply.Status,
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkUtility] Ping failed: {ex.Message}");
                return new PingResult { IsSuccess = false, Status = IPStatus.Unknown };
            }
        }

        #endregion
    }

    /// <summary>
    /// Ping 테스트 결과를 나내는 구조체
    /// </summary>
    public struct PingResult
    {
        /// <summary>
        /// Ping 테스트 성공 여부
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 왕복 시간 (밀리초)
        /// </summary>
        public long RoundtripTime { get; set; }

        /// <summary>
        /// Ping 상태
        /// </summary>
        public IPStatus Status { get; set; }
    }
}
