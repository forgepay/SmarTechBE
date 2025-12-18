using System.Net.Http.Json;

namespace MicPic.Infrastructure.Extensions;

#pragma warning disable CA1054 // URI-like parameters should not be strings

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, string requestUri, HttpContent content, Action<HttpRequestOptions> options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };

        options.Invoke(httpRequest.Options);

        var httpResponseMessage = await httpClient
            .SendAsync(httpRequest, cancellationToken);

        return httpResponseMessage;
    }

    public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient httpClient, string requestUri, TValue value, Action<HttpRequestOptions> options, CancellationToken cancellationToken)
    {
        return httpClient
            .PostAsync(requestUri, JsonContent.Create(value), options, cancellationToken);
    }

    public static HttpRequestOptions Set<TValue>(this HttpRequestOptions options, string name, TValue value)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        options.Set(new HttpRequestOptionsKey<TValue>(name), value);

        return options;
    }
}