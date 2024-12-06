using System.Net.Http;
using Hian.NetworkUtilities;
using System.Threading.Tasks;
using System.Threading;
using System;

public static class HttpClientExtensions
{
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