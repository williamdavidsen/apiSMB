using System.Net;
using System.Net.Http.Headers;

namespace API.UnitTests.TestSupport;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _handler(request, cancellationToken);
    }
}

internal static class HttpResponseFactory
{
    public static HttpResponseMessage Json(HttpStatusCode statusCode, string json, Uri? requestUri = null)
    {
        return new HttpResponseMessage(statusCode)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri ?? new Uri("https://example.com")),
            Content = new StringContent(json)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };
    }

    public static HttpResponseMessage Empty(HttpStatusCode statusCode, Uri? requestUri = null)
    {
        return new HttpResponseMessage(statusCode)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri ?? new Uri("https://example.com")),
            Content = new StringContent(string.Empty)
        };
    }
}
