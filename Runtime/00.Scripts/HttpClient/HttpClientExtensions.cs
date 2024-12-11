using System.Net.Http;
using Hian.NetworkUtilities;
using System.Threading.Tasks;
using System.Threading;
using System;

/// <summary>
/// HTTP 클라이언트에 대한 확장 메서드를 제공하는 클래스입니다.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// HTTP GET 요청을 동기적으로 수행합니다.
    /// </summary>
    /// <param name="client">HTTP 클라이언트 인스턴스</param>
    /// <param name="url">요청할 URL</param>
    /// <param name="timeoutMs">요청 제한 시간(밀리초)</param>
    /// <returns>서버로부터 받은 응답 문자열</returns>
    /// <exception cref="Exception">요청 실패 시 발생하는 예외</exception>
    public static string Get(this IHttpClient client, string url, int timeoutMs = 30000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            return Task.Run(() => client.GetAsync(url, cts.Token)).Result;
        }
        catch (AggregateException ae)
        {
            throw ae.InnerException ?? ae;
        }
    }

    /// <summary>
    /// HTTP POST 요청을 동기적으로 수행합니다.
    /// </summary>
    /// <param name="client">HTTP 클라이언트 인스턴스</param>
    /// <param name="url">요청할 URL</param>
    /// <param name="content">전송할 콘텐츠</param>
    /// <param name="contentType">콘텐츠 타입 (기본값: application/json)</param>
    /// <param name="timeoutMs">요청 제한 시간(밀리초)</param>
    /// <returns>서버로부터 받은 응답 문자열</returns>
    /// <exception cref="Exception">요청 실패 시 발생하는 예외</exception>
    public static string Post(this IHttpClient client, string url, string content, 
        string contentType = "application/json", int timeoutMs = 30000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            return Task.Run(() => client.PostAsync(url, content, contentType, cts.Token)).Result;
        }
        catch (AggregateException ae)
        {
            throw ae.InnerException ?? ae;
        }
    }

    /// <summary>
    /// HTTP PUT 요청을 동기적으로 수행합니다.
    /// </summary>
    /// <param name="client">HTTP 클라이언트 인스턴스</param>
    /// <param name="url">요청할 URL</param>
    /// <param name="content">전송할 콘텐츠</param>
    /// <param name="contentType">콘텐츠 타입 (기본값: application/json)</param>
    /// <param name="timeoutMs">요청 제한 시간(밀리초)</param>
    /// <returns>서버로부터 받은 응답 문자열</returns>
    /// <exception cref="Exception">요청 실패 시 발생하는 예외</exception>
    public static string Put(this IHttpClient client, string url, string content, 
        string contentType = "application/json", int timeoutMs = 30000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            return Task.Run(() => client.PutAsync(url, content, contentType, cts.Token)).Result;
        }
        catch (AggregateException ae)
        {
            throw ae.InnerException ?? ae;
        }
    }

    /// <summary>
    /// HTTP DELETE 요청을 동기적으로 수행합니다.
    /// </summary>
    /// <param name="client">HTTP 클라이언트 인스턴스</param>
    /// <param name="url">요청할 URL</param>
    /// <param name="timeoutMs">요청 제한 시간(밀리초)</param>
    /// <returns>서버로부터 받은 응답 문자열</returns>
    /// <exception cref="Exception">요청 실패 시 발생하는 예외</exception>
    public static string Delete(this IHttpClient client, string url, int timeoutMs = 30000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            return Task.Run(() => client.DeleteAsync(url, cts.Token)).Result;
        }
        catch (AggregateException ae)
        {
            throw ae.InnerException ?? ae;
        }
    }

    /// <summary>
    /// HTTP HEAD 요청을 동기적으로 수행합니다.
    /// </summary>
    /// <param name="client">HTTP 클라이언트 인스턴스</param>
    /// <param name="url">요청할 URL</param>
    /// <param name="timeoutMs">요청 제한 시간(밀리초)</param>
    /// <returns>HTTP 응답 메시지</returns>
    /// <exception cref="Exception">요청 실패 시 발생하는 예외</exception>
    public static HttpResponseMessage Head(this IHttpClient client, string url, int timeoutMs = 30000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            return Task.Run(() => client.HeadAsync(url, cts.Token)).Result;
        }
        catch (AggregateException ae)
        {
            throw ae.InnerException ?? ae;
        }
    }
} 