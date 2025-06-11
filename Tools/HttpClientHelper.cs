using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Tools;

/// <summary>
/// Utility class for configuring HttpClient instances with proper header handling
/// </summary>
public static class HttpClientHelper
{
    /// <summary>
    /// Configures an HttpClient with headers, properly separating content headers from request headers
    /// </summary>
    /// <param name="client">The HttpClient to configure</param>
    /// <param name="headers">JSON string containing headers to add</param>
    public static void ConfigureHeaders(HttpClient client, string? headers)
    {
        if (string.IsNullOrWhiteSpace(headers))
            return;

        try
        {
            var headerDict = JsonSerializer.Deserialize<Dictionary<string, string>>(headers) ?? new();
            ConfigureHeaders(client, headerDict);
        }
        catch (JsonException)
        {
            // Ignore header parsing errors - malformed JSON headers
        }
    }

    /// <summary>
    /// Configures an HttpClient with headers, properly separating content headers from request headers
    /// </summary>
    /// <param name="client">The HttpClient to configure</param>
    /// <param name="headers">Dictionary containing headers to add</param>
    public static void ConfigureHeaders(HttpClient client, Dictionary<string, string>? headers)
    {
        if (headers == null || headers.Count == 0)
            return;

        foreach (var header in headers)
        {
            // Skip content headers as they should be set on the content object, not request headers
            if (IsContentHeader(header.Key))
                continue;
            
            try
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            catch (InvalidOperationException)
            {
                // Handle cases where header cannot be added to request headers
                // This can happen with certain restricted headers like User-Agent, Host, etc.
            }
        }
    }

    /// <summary>
    /// Creates a StringContent for GraphQL requests with proper content type
    /// </summary>
    /// <param name="requestBody">The request body object to serialize</param>
    /// <returns>StringContent configured for GraphQL requests</returns>
    public static StringContent CreateGraphQLContent(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Determines if a header name is a content header that should be set on HttpContent rather than HttpRequestMessage
    /// </summary>
    /// <param name="headerName">The header name to check</param>
    /// <returns>True if this is a content header, false if it's a request header</returns>
    private static bool IsContentHeader(string headerName)
    {
        var contentHeaders = new[]
        {
            "Content-Type", "Content-Length", "Content-Encoding", "Content-Language",
            "Content-Location", "Content-MD5", "Content-Range", "Content-Disposition",
            "Expires", "Last-Modified"
        };
        return contentHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }
}
