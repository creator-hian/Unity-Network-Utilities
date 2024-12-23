using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

/// <summary>
/// httpbin.org의 Request Inspection API를 테스트합니다.
/// </summary>
[TestFixture]
public class HttpClientWrapperRequestInspectionTests : HttpClientWrapperTestBase
{
    #region Headers Tests
    /// <summary>
    /// 요청의 HTTP 헤더를 테스트합니다.
    /// 엔드포인트: /headers
    /// 설명: Return the incoming request's HTTP headers
    /// 응답 타입: application/json
    /// 성공 상태 코드: 200
    /// </summary>
    [Test]
    public async Task Headers_ReturnsRequestHeaders()
    {
        // Arrange
        const string headerName = "X-Custom-Header";
        const string headerValue = "TestValue";
        string expectedJson = $"{{\"headers\":{{\"X-Custom-Header\":\"{headerValue}\"}}}}";

        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);
        _wrapper.AddDefaultHeader(headerName, headerValue);

        // Act
        string result = await _wrapper.GetAsync("http://httpbin.org/headers");

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion

    #region IP Tests
    /// <summary>
    /// 요청자의 IP 주소를 테스트합니다.
    /// 엔드포인트: /ip
    /// 설명: Returns the requester's IP Address
    /// 응답 타입: application/json
    /// 성공 상태 코드: 200
    /// </summary>
    [Test]
    public async Task IP_ReturnsRequestersIPAddress()
    {
        // Arrange
        string expectedJson = "{\"origin\":\"127.0.0.1\"}";
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        string result = await _wrapper.GetAsync("http://httpbin.org/ip");

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion

    #region User-Agent Tests
    /// <summary>
    /// 요청의 User-Agent 헤더를 테스트합니다.
    /// 엔드포인트: /user-agent
    /// 설명: Return the incoming request's User-Agent header
    /// 응답 타입: application/json
    /// 성공 상태 코드: 200
    /// </summary>
    [Test]
    public async Task UserAgent_ReturnsUserAgentHeader()
    {
        // Arrange
        const string userAgent = "Unity-HttpClient/1.0";
        string expectedJson = $"{{\"user-agent\":\"{userAgent}\"}}";

        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);
        _wrapper.AddDefaultHeader("User-Agent", userAgent);

        // Act
        string result = await _wrapper.GetAsync("http://httpbin.org/user-agent");

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion

    #region Error Cases
    /// <summary>
    /// 잘못된 헤더 값으로 요청 시 실패하는 경우를 테스트합니다.
    /// </summary>
    [Test]
    public void Headers_WithInvalidValue_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.That(
            () => _wrapper.AddDefaultHeader("Invalid\nHeader", "Value"),
            Throws.Exception.TypeOf<FormatException>().With.Message.Contains("Invalid")
        );
    }

    /// <summary>
    /// 서버 오류 응답을 테스트합니다.
    /// </summary>
    [Test]
    public async Task RequestInspection_ServerError_ThrowsException()
    {
        // Arrange
        _ = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("Bad Gateway"),
            StatusCode = HttpStatusCode.BadGateway,
            ReasonPhrase = "Bad Gateway",
        };
        _mockHandler.SetupResponse(HttpStatusCode.BadGateway, "Bad Gateway");

        try
        {
            // Act
            _ = await _wrapper.GetAsync("http://httpbin.org/headers");
            Assert.Fail("Expected HttpRequestException was not thrown");
        }
        catch (HttpRequestException ex)
        {
            // Assert
            Assert.That(ex.Message, Does.Contain("BadGateway"));
            Assert.That(_mockHandler.RequestCount, Is.EqualTo(1));
        }
    }
    #endregion
}
