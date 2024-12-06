using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
/// <summary>
/// httpbin.org의 Status Code API를 테스트합니다.
/// 엔드포인트: /status/{codes}
/// 설명: 주어진 상태 코드 또는 여러 상태 코드 중 무작위로 응답을 생성합니다.
/// </summary>
[TestFixture]
public class HttpClientWrapperStatusCodeTests : HttpClientWrapperTestBase
{
    #region Common Status Code Tests
    /// <summary>
    /// DELETE 메서드의 상태 코드별 응답을 테스트합니다.
    /// 엔드포인트: /status/{codes}
    /// 응답 타입: text/plain
    /// 테스트 케이스:
    /// - 100: Informational responses
    /// - 200: Success
    /// - 300: Redirection
    /// - 400: Client Errors
    /// - 500: Server Errors
    /// </summary>
    [TestCase(100, "Informational responses")]
    [TestCase(200, "Success")]
    [TestCase(300, "Redirection")]
    [TestCase(400, "Client Errors")]
    [TestCase(500, "Server Errors")]
    public async Task DeleteStatusCode_ReturnsExpectedResponse(int statusCode, string expectedDescription)
    {
        // Arrange
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(expectedDescription),
            StatusCode = (HttpStatusCode)statusCode,
            ReasonPhrase = expectedDescription
        };
        _mockHandler.SetupResponse((HttpStatusCode)statusCode, expectedDescription);

        try
        {
            // Act
            var result = await _wrapper.DeleteAsync($"http://httpbin.org/status/{statusCode}");

            // Assert - 성공 상태 코드인 경우
            if (statusCode >= 200 && statusCode < 300)
            {
                Assert.That(result, Is.EqualTo(expectedDescription));
            }
        }
        catch (HttpRequestException ex)
        {
            // Assert - 실패 상태 코드인 경우
            if (statusCode >= 400)
            {
                var expectedStatusName = ((HttpStatusCode)statusCode).ToString();
                Assert.That(ex.Message, Does.Contain(expectedStatusName),
                    $"Expected error message to contain '{expectedStatusName}', but was: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// GET 메서드의 상태 코드별 응답을 테스트합니다.
    /// 엔드포인트: /status/{codes}
    /// 응답 타입: text/plain
    /// </summary>
    [TestCase(100, "Informational responses")]
    [TestCase(200, "Success")]
    [TestCase(300, "Redirection")]
    [TestCase(400, "Client Errors")]
    [TestCase(500, "Server Errors")]
    public async Task GetStatusCode_ReturnsExpectedResponse(int statusCode, string expectedDescription)
    {
        // Arrange
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(expectedDescription),
            StatusCode = (HttpStatusCode)statusCode,
            ReasonPhrase = expectedDescription
        };
        _mockHandler.SetupResponse((HttpStatusCode)statusCode, expectedDescription);

        try
        {
            // Act
            var result = await _wrapper.GetAsync($"http://httpbin.org/status/{statusCode}");

            // Assert - 성공 상태 코드인 경우
            if (statusCode >= 200 && statusCode < 300)
            {
                Assert.That(result, Is.EqualTo(expectedDescription));
            }
        }
        catch (HttpRequestException ex)
        {
            // Assert - 실패 상태 코드인 경우
            if (statusCode >= 400)
            {
                var expectedStatusName = ((HttpStatusCode)statusCode).ToString();
                Assert.That(ex.Message, Does.Contain(expectedStatusName),
                    $"Expected error message to contain '{expectedStatusName}', but was: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// PATCH 메서드의 상태 코드별 응답을 테스트합니다.
    /// 엔드포인트: /status/{codes}
    /// 응답 타입: text/plain
    /// </summary>
    [TestCase(100, "Informational responses")]
    [TestCase(200, "Success")]
    [TestCase(300, "Redirection")]
    [TestCase(400, "Client Errors")]
    [TestCase(500, "Server Errors")]
    public async Task PatchStatusCode_ReturnsExpectedResponse(int statusCode, string expectedDescription)
    {
        // Arrange
        const string content = "{\"test\": \"data\"}";
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(expectedDescription),
            StatusCode = (HttpStatusCode)statusCode,
            ReasonPhrase = expectedDescription
        };
        _mockHandler.SetupResponse((HttpStatusCode)statusCode, expectedDescription);

        try
        {
            // Act
            var result = await _wrapper.PatchAsync($"http://httpbin.org/status/{statusCode}", content);

            // Assert - 성공 상태 코드인 경우
            if (statusCode >= 200 && statusCode < 300)
            {
                Assert.That(result, Is.EqualTo(expectedDescription));
            }
        }
        catch (HttpRequestException ex)
        {
            // Assert - 실패 상태 코드인 경우
            if (statusCode >= 400)
            {
                var expectedStatusName = ((HttpStatusCode)statusCode).ToString();
                Assert.That(ex.Message, Does.Contain(expectedStatusName),
                    $"Expected error message to contain '{expectedStatusName}', but was: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// POST 메서드의 상태 코드별 응답을 테스트합니다.
    /// 엔드포인트: /status/{codes}
    /// 응답 타입: text/plain
    /// </summary>
    [TestCase(100, "Informational responses")]
    [TestCase(200, "Success")]
    [TestCase(300, "Redirection")]
    [TestCase(400, "Client Errors")]
    [TestCase(500, "Server Errors")]
    public async Task PostStatusCode_ReturnsExpectedResponse(int statusCode, string expectedDescription)
    {
        // Arrange
        const string content = "{\"test\": \"data\"}";
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(expectedDescription),
            StatusCode = (HttpStatusCode)statusCode,
            ReasonPhrase = expectedDescription
        };
        _mockHandler.SetupResponse((HttpStatusCode)statusCode, expectedDescription);

        try
        {
            // Act
            var result = await _wrapper.PostAsync($"http://httpbin.org/status/{statusCode}", content);

            // Assert - 성공 상태 코드인 경우
            if (statusCode >= 200 && statusCode < 300)
            {
                Assert.That(result, Is.EqualTo(expectedDescription));
            }
        }
        catch (HttpRequestException ex)
        {
            // Assert - 실패 상태 코드인 경우
            if (statusCode >= 400)
            {
                var expectedStatusName = ((HttpStatusCode)statusCode).ToString();
                Assert.That(ex.Message, Does.Contain(expectedStatusName),
                    $"Expected error message to contain '{expectedStatusName}', but was: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// PUT 메서드의 상태 코드별 응답을 테스트합니다.
    /// 엔드포인트: /status/{codes}
    /// 응답 타입: text/plain
    /// </summary>
    [TestCase(100, "Informational responses")]
    [TestCase(200, "Success")]
    [TestCase(300, "Redirection")]
    [TestCase(400, "Client Errors")]
    [TestCase(500, "Server Errors")]
    public async Task PutStatusCode_ReturnsExpectedResponse(int statusCode, string expectedDescription)
    {
        // Arrange
        const string content = "{\"test\": \"data\"}";
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(expectedDescription),
            StatusCode = (HttpStatusCode)statusCode,
            ReasonPhrase = expectedDescription
        };
        _mockHandler.SetupResponse((HttpStatusCode)statusCode, expectedDescription);

        try
        {
            // Act
            var result = await _wrapper.PutAsync($"http://httpbin.org/status/{statusCode}", content);

            // Assert - 성공 상태 코드인 경우
            if (statusCode >= 200 && statusCode < 300)
            {
                Assert.That(result, Is.EqualTo(expectedDescription));
            }
        }
        catch (HttpRequestException ex)
        {
            // Assert - 실패 상태 코드인 경우
            if (statusCode >= 400)
            {
                var expectedStatusName = ((HttpStatusCode)statusCode).ToString();
                Assert.That(ex.Message, Does.Contain(expectedStatusName),
                    $"Expected error message to contain '{expectedStatusName}', but was: {ex.Message}");
            }
        }
    }
    #endregion
}
