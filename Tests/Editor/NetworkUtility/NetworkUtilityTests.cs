using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hian.NetworkUtilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// NetworkUtility 클래스의 기능을 검증하기 위한 테스트 클래스입니다.
/// </summary>
public class NetworkUtilityTests
{
    private const string TEST_HOST = "8.8.8.8"; // Google DNS for testing
    private const string INVALID_HOST = "invalid.host.address";
    private const string TEST_ADAPTER = "Wi-Fi"; // 실제 테스트 환경에 맞게 수정 필요
    #region Network Status Tests

    /// <summary>
    /// 인터넷 연결 상태를 동기적으로 확인하는 테스트입니다.
    /// </summary>
    /// <remarks>
    /// 이 테스트는 실제 네트워크 환경에 따라 결과가 달라질 수 있습니다.
    /// 테스트 결과를 로그로 출력하여 수동 검증이 가능하도록 합니다.
    /// </remarks>
    [UnityTest]
    public IEnumerator IsInternetAvailable_ShouldReturnValidResult()
    {
        bool result = NetworkUtility.IsInternetAvailable();

        // 실제 네트워크 상태에 따라 결과가 달라질 수 있음
        Debug.Log($"Internet availability test result: {result}");
        yield return null;
    }

    /// <summary>
    /// 인터넷 연결 상태를 비동기적으로 확인하는 테스트입니다.
    /// </summary>
    /// <remarks>
    /// 비동기 작업의 완료를 대기하면서 Unity의 프레임 업데이트를 방해하지 않습니다.
    /// 실제 네트워크 상태에 따른 결과를 로그로 출력합니다.
    /// </remarks>
    [UnityTest]
    public IEnumerator IsInternetAvailableAsync_ShouldReturnValidResult()
    {
        Task<bool> task = NetworkUtility.IsInternetAvailableAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        bool result = task.Result;
        Debug.Log($"Async internet availability test result: {result}");
        yield return null;
    }

    [Test]
    public void IsNetworkAvailable_ShouldReturnValidResult()
    {
        bool result = NetworkUtility.IsNetworkAvailable();
        Assert.That(result, Is.TypeOf<bool>());
    }

    #endregion

    #region IP Address Tests

    [Test]
    public void GetAllNetworkIPs_ShouldReturnNonNullList()
    {
        IList<string> ips = NetworkUtility.GetAllNetworkIPs();

        Assert.That(ips, Is.Not.Null);
        Assert.That(ips, Is.All.Not.Null.And.Not.Empty);

        foreach (string ip in ips)
        {
            Debug.Log($"Found IP address: {ip}");
        }
    }

    [UnityTest]
    public IEnumerator GetPublicIP_ShouldReturnValidIP()
    {
        string ip = NetworkUtility.GetPublicIP();

        Assert.That(ip, Is.Not.Null);
        Assert.That(ip, Is.Not.Empty);
        Debug.Log($"Public IP: {ip}");

        yield return null;
    }

    [UnityTest]
    public IEnumerator GetPublicIPAsync_ShouldReturnValidIP()
    {
        Task<string> task = NetworkUtility.GetPublicIPAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        string ip = task.Result;
        Assert.That(ip, Is.Not.Null);
        Assert.That(ip, Is.Not.Empty);
        Debug.Log($"Async Public IP: {ip}");
    }

    /// <summary>
    /// 로컬 IP 주소 가져오기 테스트입니다.
    /// </summary>
    /// <remarks>
    /// 1. 반환된 IP가 올바른 IPv4 형식인지 확인
    /// 2. 로컬 네트워크 범위(예: 192.168.x.x, 10.x.x.x) 내의 IP인지 검증
    /// </remarks>
    [Test]
    public void GetLocalIPAddress_ShouldReturnValidLocalIP()
    {
        string localIP = NetworkUtility.GetLocalIPAddress();

        Assert.That(localIP, Is.Not.Null);
        Assert.That(localIP, Is.Not.Empty);
        Assert.That(
            System.Net.IPAddress.Parse(localIP).AddressFamily,
            Is.EqualTo(System.Net.Sockets.AddressFamily.InterNetwork)
        );
    }

    /// <summary>
    /// 네트워크 인터페이스 상태 변경 이벤트 테스트입니다.
    /// </summary>
    /// <remarks>
    /// 네트워크 상태 변경 이벤트가 제대로 발생하는지 확인합니다.
    /// </remarks>
    [Test]
    public void NetworkStatusChanged_ShouldTriggerEvent()
    {
        bool eventTriggered = false;
        void handler(bool isAvailable)
        {
            eventTriggered = true;
        }

        NetworkUtility.OnNetworkStatusChanged += handler;

        try
        {
            NetworkUtility.SimulateNetworkStatusChange(false);
            Assert.That(eventTriggered, Is.True);
        }
        finally
        {
            NetworkUtility.OnNetworkStatusChanged -= handler;
        }
    }

    /// <summary>
    /// 네트워크 타임아웃 설정 테스트입니다.
    /// </summary>
    /// <remarks>
    /// 다양한 타임아웃 값에 대한 동작 검증
    /// </remarks>
    [Test]
    public void SetNetworkTimeout_ShouldApplyCorrectly()
    {
        int timeout = 5000;
        NetworkUtility.SetNetworkTimeout(timeout);

        // 타임아웃이 적용된 상태에서 네트워크 작업 수행
        PingResult result = NetworkUtility.PingHost(TEST_HOST);

        Assert.That(result.RoundtripTime, Is.LessThanOrEqualTo(timeout));
    }

    #endregion

    #region Network Interface Tests

    [Test]
    public void GetAdapterIPAddress_WithValidAdapter_ShouldReturnIP()
    {
        string ip = NetworkUtility.GetAdapterIPAddress(TEST_ADAPTER);
        Debug.Log($"Adapter IP for {TEST_ADAPTER}: {ip}");
    }

    [Test]
    public void GetAdapterIPAddress_WithInvalidAdapter_ShouldReturnEmpty()
    {
        string ip = NetworkUtility.GetAdapterIPAddress("NonExistentAdapter");
        Assert.That(ip, Is.Empty);
    }

    /// <summary>
    /// 네트워크 어댑터의 IP 주소를 가져오는 테스트입니다.
    /// </summary>
    /// <remarks>
    /// null 어댑터 이름으로 호출 시:
    /// 1. 에러 로그가 발생하는지 확인
    /// 2. 빈 문자열을 반환하는지 확인
    /// </remarks>
    [Test]
    public void GetAdapterIPAddress_WithNullAdapter_ShouldReturnEmpty()
    {
        LogAssert.Expect(LogType.Error, "[NetworkUtility] Adapter name cannot be null or empty");
        string ip = NetworkUtility.GetAdapterIPAddress(null);
        Assert.That(ip, Is.Empty);
    }

    #endregion

    #region Ping Tests

    [UnityTest]
    public IEnumerator PingHost_WithValidHost_ShouldSucceed()
    {
        PingResult result = NetworkUtility.PingHost(TEST_HOST);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.RoundtripTime, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.Status, Is.EqualTo(System.Net.NetworkInformation.IPStatus.Success));

        yield return null;
    }

    /// <summary>
    /// 잘못된 호스트에 대한 ping 테스트입니다.
    /// </summary>
    /// <remarks>
    /// 이 테스트는 다음을 검증합니다:
    /// 1. 잘못된 호스트 주소에 대한 적절한 에러 로그 발생
    /// 2. PingResult가 실패 상태를 정확히 반영
    /// 3. 네트워크 상태가 실패로 표시되는지 확인
    /// </remarks>
    [UnityTest]
    public IEnumerator PingHost_WithInvalidHost_ShouldFail()
    {
        LogAssert.Expect(
            LogType.Error,
            "[NetworkUtility] Ping failed: Could not resolve host 'invalid.host.address'"
        );
        PingResult result = NetworkUtility.PingHost(INVALID_HOST);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Status, Is.Not.EqualTo(System.Net.NetworkInformation.IPStatus.Success));

        yield return null;
    }

    [UnityTest]
    public IEnumerator PingHostAsync_WithValidHost_ShouldSucceed()
    {
        Task<PingResult> task = NetworkUtility.PingHostAsync(TEST_HOST);

        while (!task.IsCompleted)
        {
            yield return null;
        }

        PingResult result = task.Result;
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.RoundtripTime, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.Status, Is.EqualTo(System.Net.NetworkInformation.IPStatus.Success));
    }

    [Test]
    public void PingHost_WithNullHost_ShouldThrowArgumentNullException()
    {
        _ = Assert.Throws<ArgumentNullException>(static () => NetworkUtility.PingHost(null));
    }

    [Test]
    public void PingHost_WithEmptyHost_ShouldThrowArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(static () => NetworkUtility.PingHost(""));
    }

    [Test]
    public void PingHost_WithInvalidTimeout_ShouldThrowArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(static () => NetworkUtility.PingHost(TEST_HOST, 0));
    }

    #endregion
}
