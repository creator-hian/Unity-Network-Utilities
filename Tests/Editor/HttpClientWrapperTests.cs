using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Hian.NetworkUtilities;
using System.Text;

// 기본 설정을 위한 추상 기본 클래스
public abstract class HttpClientWrapperTestBase
{
    protected HttpClientWrapper _wrapper;
    protected HttpClientSettings _settings;
    protected MockHttpMessageHandler _mockHandler;

    [SetUp]
    public virtual void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _settings = new HttpClientSettings
        {
            Handler = _mockHandler,
            RetryCount = 3,
            RetryDelay = TimeSpan.FromMilliseconds(100),
            MaxConcurrentRequests = 2,
            MaxMetricsCount = 100,
            Timeout = TimeSpan.FromSeconds(1),
            RetryableStatusCodes = new HashSet<HttpStatusCode>
                {
                    HttpStatusCode.InternalServerError,
                    HttpStatusCode.ServiceUnavailable
                }
        };

        _wrapper = new HttpClientWrapper(_settings);
    }

    [TearDown]
    public virtual void Cleanup()
    {
        _wrapper?.Dispose();
    }
}

/// <summary>
/// httpbin.org API 명세에 따른 HTTP 메서드 테스트
/// </summary>
[TestFixture]
public class HttpClientWrapperHttpMethodTests : HttpClientWrapperTestBase
{
    #region DELETE Tests
    /// <summary>
    /// DELETE 메서드의 application/json 응답을 테스트합니다.
    /// 이미지의 DELETE 엔드포인트 명세:
    /// - 엔드포인트: /delete
    /// - 파라미터: 없음
    /// - 응답 타입: application/json
    /// - 성공 상태 코드: 200
    /// - 응답 내용: The request's DELETE parameters
    /// </summary>
    [Test]
    public async Task DeleteAsync_ReturnsJsonResponse()
    {
        // Arrange
        var expectedJson = "{\"status\":\"success\"}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        var result = await _wrapper.DeleteAsync("http://httpbin.org/delete");

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion

    #region GET Tests
    /// <summary>
    /// GET 메서드의 application/json 응답을 테스트합니다.
    /// 이미지의 GET 엔드포인트 명세:
    /// - 엔드포인트: /get
    /// - 파라미터: 없음
    /// - 응답 타입: application/json
    /// - 성공 상태 코드: 200
    /// - 응답 내용: The request's query parameters
    /// </summary>
    [Test]
    public async Task GetAsync_ReturnsJsonResponse()
    {
        // Arrange
        var expectedJson = "{\"query_params\":{}}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        var result = await _wrapper.GetAsync("http://httpbin.org/get");

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion

    #region PATCH Tests
    /// <summary>
    /// PATCH 메서드의 application/json 응답을 테스트합니다.
    /// 이미지의 PATCH 엔드포인트 명세:
    /// - 엔드포인트: /patch
    /// - 파라미터: 없음
    /// - 응답 타입: application/json
    /// - 성공 상태 코드: 200
    /// - 응답 내용: The request's PATCH parameters
    /// </summary>
    [Test]
    public async Task PatchAsync_ReturnsJsonResponse()
    {
        // Arrange
        var requestContent = "{\"test\": \"data\"}";
        var expectedJson = "{\"json\":{\"test\":\"data\"}}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        var result = await _wrapper.PatchAsync("http://httpbin.org/patch", requestContent);

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion

    #region POST Tests
    /// <summary>
    /// POST 메서드의 application/json 응답을 테스트합니다.
    /// 이미지의 POST 엔드포인트 명세:
    /// - 엔드포인트: /post
    /// - 파라미터: 없음
    /// - 응답 타입: application/json
    /// - 성공 상태 코드: 200
    /// - 응답 내용: The request's POST parameters
    /// </summary>
    [Test]
    public async Task PostAsync_ReturnsJsonResponse()
    {
        // Arrange
        var requestContent = "{\"test\": \"data\"}";
        var expectedJson = "{\"json\":{\"test\":\"data\"}}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        var result = await _wrapper.PostAsync("http://httpbin.org/post", requestContent);

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion

    #region PUT Tests
    /// <summary>
    /// PUT 메서드의 application/json 응답을 테스트합니다.
    /// 이미지의 PUT 엔드포인트 명세:
    /// - 엔드포인트: /put
    /// - 파라미터: 없음
    /// - 응답 타입: application/json
    /// - 성공 상태 코드: 200
    /// - 응답 내용: The request's PUT parameters
    /// </summary>
    [Test]
    public async Task PutAsync_ReturnsJsonResponse()
    {
        // Arrange
        var requestContent = "{\"test\": \"data\"}";
        var expectedJson = "{\"json\":{\"test\":\"data\"}}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        var result = await _wrapper.PutAsync("http://httpbin.org/put", requestContent);

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion
}

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
        var expectedJson = $"{{\"headers\":{{\"X-Custom-Header\":\"{headerValue}\"}}}}";
        
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);
        _wrapper.AddDefaultHeader(headerName, headerValue);

        // Act
        var result = await _wrapper.GetAsync("http://httpbin.org/headers");

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
        var expectedJson = "{\"origin\":\"127.0.0.1\"}";
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        var result = await _wrapper.GetAsync("http://httpbin.org/ip");

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
        var expectedJson = $"{{\"user-agent\":\"{userAgent}\"}}";
        
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);
        _wrapper.AddDefaultHeader("User-Agent", userAgent);

        // Act
        var result = await _wrapper.GetAsync("http://httpbin.org/user-agent");

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
        Assert.That(() => _wrapper.AddDefaultHeader("Invalid\nHeader", "Value"),
            Throws.Exception.TypeOf<FormatException>()
            .With.Message.Contains("Invalid"));
    }

    /// <summary>
    /// 서버 오류 응답을 테스트합니다.
    /// </summary>
    [Test]
    public async Task RequestInspection_ServerError_ThrowsException()
    {
        // Arrange
        var errorResponse = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("Bad Gateway"),
            StatusCode = HttpStatusCode.BadGateway,
            ReasonPhrase = "Bad Gateway"
        };
        _mockHandler.SetupResponse(HttpStatusCode.BadGateway, "Bad Gateway");

        try
        {
            // Act
            await _wrapper.GetAsync("http://httpbin.org/headers");
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


// Authentication Tests
[TestFixture]
public class HttpClientWrapperAuthenticationTests : HttpClientWrapperTestBase
{
    [Test]
    public async Task RequestWithBasicAuth_SuccessfulRequest_ReturnsContent()
    {
        // Arrange
        const string username = "testuser";
        const string password = "testpass";
        const string expectedResponse = "Authenticated";
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);
        
        // Add Basic Auth header
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        _wrapper.AddDefaultHeader("Authorization", $"Basic {credentials}");

        // Act
        var result = await _wrapper.GetAsync("http://httpbin.org/basic-auth/testuser/testpass");

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task RequestWithBearerToken_SuccessfulRequest_ReturnsContent()
    {
        // Arrange
        const string token = "test-token";
        const string expectedResponse = "Authenticated";
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);
        
        // Add Bearer token header
        _wrapper.AddDefaultHeader("Authorization", $"Bearer {token}");

        // Act
        var result = await _wrapper.GetAsync("http://httpbin.org/bearer");

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
    }
}

// Core Functionality Tests
[TestFixture]
public class HttpClientWrapperCoreFunctionalityTests : HttpClientWrapperTestBase
{
    [Test]
    public async Task GetAsync_SuccessfulRequest_ReturnsContent()
    {
        // Arrange
        const string expectedContent = "Test Response";
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedContent);

        // Act
        var result = await _wrapper.GetAsync("http://httpbin.org/get");

        // Assert
        Assert.That(result, Is.EqualTo(expectedContent));
        Assert.That(_wrapper.TotalRequestCount, Is.EqualTo(1));
        Assert.That(_wrapper.FailedRequestCount, Is.EqualTo(0));
    }

    [Test]
    public void GetAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        _wrapper.Dispose();

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await _wrapper.GetAsync("http://httpbin.org/get"));
    }

    [Test]
    public async Task GetAsync_WithRetry_EventuallySucceeds()
    {
        // Arrange
        const string expectedContent = "Success";
        var responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.InternalServerError) 
            { 
                Content = new StringContent("Error 1"),
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = "Internal Server Error"
            },
            new HttpResponseMessage(HttpStatusCode.InternalServerError) 
            { 
                Content = new StringContent("Error 2"),
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = "Internal Server Error"
            },
            new HttpResponseMessage(HttpStatusCode.OK) 
            { 
                Content = new StringContent(expectedContent),
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "OK"
            }
        };
        _mockHandler.SetupResponses(responses);

        // Act
        var result = await _wrapper.GetAsync("http://httpbin.org/status/500");
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedContent));
        Assert.That(_wrapper.TotalRequestCount, Is.EqualTo(1));
        Assert.That(_wrapper.FailedRequestCount, Is.EqualTo(0));
        Assert.That(_mockHandler.RequestCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetAsync_NonRetryableError_ThrowsImmediately()
    {
        // Arrange
        _wrapper.SetTimeout(500);
        var settings = _settings.Clone();
        settings.Handler = _mockHandler;
        settings.RetryableStatusCodes = new HashSet<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable
        };
        _wrapper = new HttpClientWrapper(settings);

        var notFoundResponse = new HttpResponseMessage(HttpStatusCode.NotFound) 
        { 
            Content = new StringContent("Not Found"),
            ReasonPhrase = "Not Found",
            StatusCode = HttpStatusCode.NotFound
        };
        _mockHandler.SetupResponse(HttpStatusCode.NotFound, "Not Found");

        try
        {
            // Act
            await _wrapper.GetAsync("http://httpbin.org/status/404");
            Assert.Fail("Expected HttpRequestException was not thrown");
        }
        catch (HttpRequestException ex)
        {
            // Assert
            Assert.That(_mockHandler.RequestCount, Is.EqualTo(1));
            Assert.That(ex.Message, Does.Contain("NotFound"));
        }
    }
}

// Concurrency and Performance Tests
[TestFixture]
public class HttpClientWrapperConcurrencyTests : HttpClientWrapperTestBase
{
    [Test]
    public async Task ConcurrentRequests_RespectMaxLimit()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Response 1") },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Response 2") },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Response 3") }
        };
        _mockHandler.SetupResponses(responses);

        // Act
        var task1 = Task.Run(() => _wrapper.GetAsync("http://httpbin.org/get"));
        var task2 = Task.Run(() => _wrapper.GetAsync("http://httpbin.org/delay/1"));
        
        // 첫 두 요청이 시작되기를 기다림
        await Task.Delay(100);
        
        // Assert - 첫 두 요청이 동시에 실행 중인지 확인
        Assert.That(_mockHandler.ActiveRequestCount, Is.EqualTo(2), "Expected 2 concurrent requests");
        
        // 세 번째 요청 시작 (이 요청은 대기해야 함)
        var task3 = Task.Run(() => _wrapper.GetAsync("http://httpbin.org/delay/2"));
        
        // 모든 요청 완료 대기
        await Task.WhenAll(task1, task2, task3);
        
        // 최종 Assert
        Assert.That(_mockHandler.RequestCount, Is.EqualTo(3), "Expected total of 3 requests");
        Assert.That(_mockHandler.ActiveRequestCount, Is.EqualTo(0), "Expected no active requests after completion");
    }

    [Test]
    public async Task Metrics_TrackSuccessAndFailure()
    {
        // Arrange
        var responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK) 
            { 
                Content = new StringContent("Success"),
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "OK"
            },
            new HttpResponseMessage(HttpStatusCode.NotFound) 
            { 
                Content = new StringContent("Not Found"),
                StatusCode = HttpStatusCode.NotFound,
                ReasonPhrase = "Not Found"
            }
        };
        _mockHandler.SetupResponses(responses);

        // Act - 첫 번째 요청 (성공)
        var result = await _wrapper.GetAsync("http://httpbin.org/get");
        Assert.That(result, Is.EqualTo("Success"), "First request should succeed");

        // Act - 두 번째 요청 (실패)
        try
        {
            await _wrapper.GetAsync("http://httpbin.org/status/404");
            Assert.Fail("Expected HttpRequestException was not thrown");
        }
        catch (HttpRequestException ex)
        {
            Assert.That(ex.Message, Does.Contain("NotFound"), "Exception should contain status code");
        }

        // Assert - 메트릭스 확인
        Assert.That(_wrapper.TotalRequestCount, Is.EqualTo(2), "Should have 2 total requests");
        Assert.That(_wrapper.FailedRequestCount, Is.EqualTo(1), "Should have 1 failed request");
        Assert.That(_wrapper.AverageResponseTime, Is.GreaterThan(TimeSpan.Zero), "Should have positive average response time");
    }
}

// Header Management Tests
[TestFixture]
public class HttpClientWrapperHeaderManagementTests : HttpClientWrapperTestBase
{
    [Test]
    public void DefaultHeaders_AddAndRemove_WorksCorrectly()
    {
        // Arrange
        const string headerName = "X-Test-Header";
        const string headerValue = "TestValue";

        // Act & Assert
        Assert.DoesNotThrow(() => _wrapper.AddDefaultHeader(headerName, headerValue));
        Assert.DoesNotThrow(() => _wrapper.RemoveDefaultHeader(headerName));
    }

    [Test]
    public void DefaultHeaders_AddWithEmptyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _wrapper.AddDefaultHeader("", "value"));
    }
}

// HTTP Method Implementation Tests
[TestFixture]
public class HttpClientWrapperImplementationTests : HttpClientWrapperTestBase
{
    [Test]
    public async Task PostAsync_SuccessfulRequest_ReturnsContent()
    {
        // Arrange
        const string requestContent = "{\"test\": \"data\"}";
        const string expectedResponse = "Success";
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _wrapper.PostAsync("http://httpbin.org/post", requestContent);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
        Assert.That(_wrapper.TotalRequestCount, Is.EqualTo(1));
    }

    [Test]
    public async Task PutAsync_SuccessfulRequest_ReturnsContent()
    {
        // Arrange
        const string requestContent = "{\"test\": \"data\"}";
        const string expectedResponse = "Success";
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _wrapper.PutAsync("http://httpbin.org/put", requestContent);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
        Assert.That(_wrapper.TotalRequestCount, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteAsync_SuccessfulRequest_ReturnsContent()
    {
        // Arrange
        const string expectedResponse = "Deleted";
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _wrapper.DeleteAsync("http://httpbin.org/delete");

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
        Assert.That(_wrapper.TotalRequestCount, Is.EqualTo(1));
    }
}

// Mock Handler
public class MockHttpMessageHandler : HttpClientHandler
{
    private Queue<HttpResponseMessage> _responses = new Queue<HttpResponseMessage>();
    private TaskCompletionSource<bool> _delayTask;
    public int RequestCount { get; private set; }
    private int _activeRequestCount;
    public int ActiveRequestCount => _activeRequestCount;

    public void SetupResponse(HttpStatusCode statusCode, string content = null)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            StatusCode = statusCode,
            ReasonPhrase = statusCode.ToString()
        };
        if (content != null)
            response.Content = new StringContent(content);
        _responses.Clear();
        _responses.Enqueue(response);
    }

    public void SetupResponses(IEnumerable<HttpResponseMessage> responses)
    {
        _responses.Clear();
        foreach (var response in responses)
            _responses.Enqueue(response);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestCount++;
        Interlocked.Increment(ref _activeRequestCount);

        try
        {
            await Task.Delay(200, cancellationToken);

            if (_responses.Count == 0)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Default Response"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "OK"
                };
            }

            return _responses.Dequeue();
        }
        finally
        {
            Interlocked.Decrement(ref _activeRequestCount);
        }
    }
}
