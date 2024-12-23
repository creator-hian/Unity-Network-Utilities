using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hian.NetworkUtilities;
using NUnit.Framework;

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
                HttpStatusCode.ServiceUnavailable,
            },
        };

        _wrapper = new HttpClientWrapper(_settings);
    }

    [TearDown]
    public virtual void Cleanup()
    {
        _wrapper?.Dispose();
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
        string result = await _wrapper.GetAsync("http://httpbin.org/get");

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
        _ = Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await _wrapper.GetAsync("http://httpbin.org/get")
        );
    }

    [Test]
    public async Task GetAsync_WithRetry_EventuallySucceeds()
    {
        // Arrange
        const string expectedContent = "Success";
        HttpResponseMessage[] responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Error 1"),
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = "Internal Server Error",
            },
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Error 2"),
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = "Internal Server Error",
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedContent),
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "OK",
            },
        };
        _mockHandler.SetupResponses(responses);

        // Act
        string result = await _wrapper.GetAsync("http://httpbin.org/status/500");

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
        HttpClientSettings settings = _settings.Clone();
        settings.Handler = _mockHandler;
        settings.RetryableStatusCodes = new HashSet<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable,
        };
        _wrapper = new HttpClientWrapper(settings);
        _ = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Not Found"),
            ReasonPhrase = "Not Found",
            StatusCode = HttpStatusCode.NotFound,
        };
        _mockHandler.SetupResponse(HttpStatusCode.NotFound, "Not Found");

        try
        {
            // Act
            _ = await _wrapper.GetAsync("http://httpbin.org/status/404");
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
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        HttpResponseMessage[] responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Response 1"),
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Response 2"),
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Response 3"),
            },
        };
        _mockHandler.SetupResponses(responses);

        // Act
        Task<string> task1 = Task.Run(() => _wrapper.GetAsync("http://httpbin.org/get"));
        Task<string> task2 = Task.Run(() => _wrapper.GetAsync("http://httpbin.org/delay/1"));

        // 첫 두 요청이 시작되기를 기다림
        await Task.Delay(100);

        // Assert - 첫 두 요청이 동시에 실행 중인지 확인
        Assert.That(
            _mockHandler.ActiveRequestCount,
            Is.EqualTo(2),
            "Expected 2 concurrent requests"
        );

        // 세 번째 요청 시작 (이 요청은 대기해야 함)
        Task<string> task3 = Task.Run(() => _wrapper.GetAsync("http://httpbin.org/delay/2"));

        // 모든 요청 완료 대기
        _ = await Task.WhenAll(task1, task2, task3);

        // 최종 Assert
        Assert.That(_mockHandler.RequestCount, Is.EqualTo(3), "Expected total of 3 requests");
        Assert.That(
            _mockHandler.ActiveRequestCount,
            Is.EqualTo(0),
            "Expected no active requests after completion"
        );
    }

    [Test]
    public async Task Metrics_TrackSuccessAndFailure()
    {
        // Arrange
        HttpResponseMessage[] responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Success"),
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "OK",
            },
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not Found"),
                StatusCode = HttpStatusCode.NotFound,
                ReasonPhrase = "Not Found",
            },
        };
        _mockHandler.SetupResponses(responses);

        // Act - 첫 번째 요청 (성공)
        string result = await _wrapper.GetAsync("http://httpbin.org/get");
        Assert.That(result, Is.EqualTo("Success"), "First request should succeed");

        // Act - 두 번째 요청 (실패)
        try
        {
            _ = await _wrapper.GetAsync("http://httpbin.org/status/404");
            Assert.Fail("Expected HttpRequestException was not thrown");
        }
        catch (HttpRequestException ex)
        {
            Assert.That(
                ex.Message,
                Does.Contain("NotFound"),
                "Exception should contain status code"
            );
        }

        // Assert - 메트릭스 확인
        Assert.That(_wrapper.TotalRequestCount, Is.EqualTo(2), "Should have 2 total requests");
        Assert.That(_wrapper.FailedRequestCount, Is.EqualTo(1), "Should have 1 failed request");
        Assert.That(
            _wrapper.AverageResponseTime,
            Is.GreaterThan(TimeSpan.Zero),
            "Should have positive average response time"
        );
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
        _ = Assert.Throws<ArgumentNullException>(() => _wrapper.AddDefaultHeader("", "value"));
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
        string result = await _wrapper.PostAsync("http://httpbin.org/post", requestContent);

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
        string result = await _wrapper.PutAsync("http://httpbin.org/put", requestContent);

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
        string result = await _wrapper.DeleteAsync("http://httpbin.org/delete");

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
        Assert.That(_wrapper.TotalRequestCount, Is.EqualTo(1));
    }
}

// Mock Handler
public class MockHttpMessageHandler : HttpClientHandler
{
    private Queue<HttpResponseMessage> _responses = new Queue<HttpResponseMessage>();

    public int RequestCount { get; private set; }
    private int _activeRequestCount;
    public int ActiveRequestCount => _activeRequestCount;

    public void SetupResponse(HttpStatusCode statusCode, string content = null)
    {
        HttpResponseMessage response = new HttpResponseMessage(statusCode)
        {
            StatusCode = statusCode,
            ReasonPhrase = statusCode.ToString(),
        };
        if (content != null)
        {
            response.Content = new StringContent(content);
        }

        _responses.Clear();
        _responses.Enqueue(response);
    }

    public void SetupResponses(IEnumerable<HttpResponseMessage> responses)
    {
        _responses.Clear();
        foreach (HttpResponseMessage response in responses)
        {
            _responses.Enqueue(response);
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        RequestCount++;
        _ = Interlocked.Increment(ref _activeRequestCount);

        try
        {
            await Task.Delay(200, cancellationToken);

            if (_responses.Count == 0)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Default Response"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "OK",
                };
            }

            return _responses.Dequeue();
        }
        finally
        {
            _ = Interlocked.Decrement(ref _activeRequestCount);
        }
    }
}
