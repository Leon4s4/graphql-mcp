using Graphql.Mcp.Tools;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Implementation of IGraphQLHttpClient that uses IHttpClientFactory for proper HttpClient lifecycle management
/// </summary>
public class GraphQlHttpClient : IGraphQlHttpClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GraphQlHttpClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Creates a configured HttpClient for GraphQL operations
    /// </summary>
    /// <param name="headers">Optional headers to add (as JSON string)</param>
    /// <param name="timeout">Optional timeout override</param>
    /// <returns>Configured HttpClient</returns>
    public HttpClient CreateClient(string? headers = null, TimeSpan? timeout = null)
    {
        var client = _httpClientFactory.CreateClient();

        if (timeout.HasValue)
        {
            client.Timeout = timeout.Value;
        }

        HttpClientHelper.ConfigureHeaders(client, headers);
        return client;
    }

    /// <summary>
    /// Creates a configured HttpClient for GraphQL operations
    /// </summary>
    /// <param name="headers">Optional headers to add (as dictionary)</param>
    /// <param name="timeout">Optional timeout override</param>
    /// <returns>Configured HttpClient</returns>
    public HttpClient CreateClient(Dictionary<string, string>? headers, TimeSpan? timeout = null)
    {
        var client = _httpClientFactory.CreateClient();

        if (timeout.HasValue)
        {
            client.Timeout = timeout.Value;
        }

        HttpClientHelper.ConfigureHeaders(client, headers);
        return client;
    }
}