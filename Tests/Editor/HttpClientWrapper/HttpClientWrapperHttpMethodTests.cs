using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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
        string expectedJson = "{\"status\":\"success\"}";
        _ = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK",
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        string result = await _wrapper.DeleteAsync("http://httpbin.org/delete");

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
        string expectedJson = "{\"query_params\":{}}";
        _ = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK",
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        string result = await _wrapper.GetAsync("http://httpbin.org/get");

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
        string requestContent = "{\"test\": \"data\"}";
        string expectedJson = "{\"json\":{\"test\":\"data\"}}";
        _ = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK",
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        string result = await _wrapper.PatchAsync("http://httpbin.org/patch", requestContent);

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
        string requestContent = "{\"test\": \"data\"}";
        string expectedJson = "{\"json\":{\"test\":\"data\"}}";
        _ = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK",
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        string result = await _wrapper.PostAsync("http://httpbin.org/post", requestContent);

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
        string requestContent = "{\"test\": \"data\"}";
        string expectedJson = "{\"json\":{\"test\":\"data\"}}";
        _ = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK",
        };
        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedJson);

        // Act
        string result = await _wrapper.PutAsync("http://httpbin.org/put", requestContent);

        // Assert
        Assert.That(result, Is.EqualTo(expectedJson));
    }
    #endregion
}
