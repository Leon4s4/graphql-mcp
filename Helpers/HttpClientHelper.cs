using System.Net;
using System.Text;
using System.Text.Json;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Utility class for configuring HttpClient instances with proper header handling
/// </summary>
public static class HttpClientHelper
{
    private static readonly Lazy<HttpClient> StaticHttpClient = new(() => new HttpClient());

    /// <summary>
    /// Creates a configured HttpClient for GraphQL operations that can be used in static contexts
    /// </summary>
    /// <param name="headers">Optional headers to add (as JSON string)</param>
    /// <param name="timeout">Optional timeout override</param>
    /// <returns>Configured HttpClient</returns>
    public static HttpClient CreateStaticClient(string? headers = null, TimeSpan? timeout = null)
    {
        var client = new HttpClient();

        if (timeout.HasValue)
        {
            client.Timeout = timeout.Value;
        }

        ConfigureHeaders(client, headers);
        return client;
    }

    /// <summary>
    /// Creates a configured HttpClient for GraphQL operations that can be used in static contexts
    /// </summary>
    /// <param name="headers">Optional headers to add (as dictionary)</param>
    /// <param name="timeout">Optional timeout override</param>
    /// <returns>Configured HttpClient</returns>
    public static HttpClient CreateStaticClient(Dictionary<string, string>? headers, TimeSpan? timeout = null)
    {
        var client = new HttpClient();

        if (timeout.HasValue)
        {
            client.Timeout = timeout.Value;
        }

        ConfigureHeaders(client, headers);
        return client;
    }

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
            if (IsContentHeader(header.Key))
                continue;

            try
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    /// <summary>
    /// Creates a StringContent for GraphQL requests with proper content type
    /// </summary>
    /// <param name="requestBody">The request body object to serialize</param>
    /// <returns>StringContent configured for GraphQL requests</returns>
    public static StringContent CreateGraphQlContent(object requestBody)
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

    /// <summary>
    /// Executes a GraphQL request with comprehensive error handling and connection validation
    /// </summary>
    /// <param name="endpoint">The GraphQL endpoint URL</param>
    /// <param name="requestBody">The GraphQL request body (query, variables, etc.)</param>
    /// <param name="headers">Optional headers to add (as JSON string)</param>
    /// <param name="timeout">Optional timeout override</param>
    /// <returns>GraphQLResponse containing either success data or detailed error information</returns>
    public static async Task<GraphQlResponse> ExecuteGraphQlRequestAsync(string endpoint, object requestBody, string? headers = null, TimeSpan? timeout = null)
    {
        return await ExecuteGraphQlRequestAsync(endpoint, requestBody, ParseHeaders(headers), timeout);
    }

    /// <summary>
    /// Executes a GraphQL request with comprehensive error handling and connection validation
    /// </summary>
    /// <param name="endpoint">The GraphQL endpoint URL</param>
    /// <param name="requestBody">The GraphQL request body (query, variables, etc.)</param>
    /// <param name="headers">Optional headers to add (as dictionary)</param>
    /// <param name="timeout">Optional timeout override</param>
    /// <returns>GraphQLResponse containing either success data or detailed error information</returns>
    public static async Task<GraphQlResponse> ExecuteGraphQlRequestAsync(string endpoint, object requestBody, Dictionary<string, string>? headers = null, TimeSpan? timeout = null)
    {
        HttpResponseMessage? response = null;

        try
        {
            using var client = CreateStaticClient(headers, timeout);
            var content = CreateGraphQlContent(requestBody);

            // Validate endpoint URL
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            {
                return GraphQlResponse.ConnectionError($"Invalid endpoint URL: {endpoint}");
            }

            // Execute the request
            response = await client.PostAsync(endpoint, content);

            // Ensure success status code - this will throw for non-success status codes
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse and validate GraphQL response
            return ParseGraphQlResponse(responseContent);
        }
        catch (HttpRequestException) when (response?.IsSuccessStatusCode == false)
        {
            // HTTP error response (4xx, 5xx) - EnsureSuccessStatusCode threw
            var errorContent = "";
            try
            {
                errorContent = await response.Content.ReadAsStringAsync();
            }
            catch
            {
            }

            return GraphQlResponse.HttpError(response.StatusCode, response.ReasonPhrase ?? "Unknown error", errorContent);
        }
        catch (HttpRequestException ex)
        {
            // Connection-related errors (network issues, DNS resolution, etc.)
            return GraphQlResponse.ConnectionError($"Cannot connect to GraphQL endpoint '{endpoint}': {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            // Request timeout
            return GraphQlResponse.ConnectionError($"Request to GraphQL endpoint '{endpoint}' timed out");
        }
        catch (TaskCanceledException)
        {
            // Request was cancelled
            return GraphQlResponse.ConnectionError($"Request to GraphQL endpoint '{endpoint}' was cancelled");
        }
        catch (Exception ex)
        {
            // Unexpected errors
            return GraphQlResponse.UnexpectedError($"Unexpected error calling GraphQL endpoint '{endpoint}': {ex.Message}");
        }
        finally
        {
            response?.Dispose();
        }
    }

    /// <summary>
    /// Parses a GraphQL response and determines if it contains errors
    /// </summary>
    private static GraphQlResponse ParseGraphQlResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            // Check for GraphQL errors
            if (root.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Array &&
                errors.GetArrayLength() > 0)
            {
                return GraphQlResponse.GraphQlErrors(responseContent);
            }

            return GraphQlResponse.Success(responseContent);
        }
        catch (JsonException)
        {
            // If JSON parsing fails, treat as successful raw response
            return GraphQlResponse.Success(responseContent);
        }
    }

    /// <summary>
    /// Parses headers from JSON string to dictionary
    /// </summary>
    private static Dictionary<string, string>? ParseHeaders(string? headers)
    {
        if (string.IsNullOrWhiteSpace(headers))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(headers);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>
/// Represents the result of a GraphQL request with detailed error categorization
/// </summary>
public class GraphQlResponse
{
    public bool IsSuccess { get; private set; }
    public string? Content { get; private set; }
    public GraphQlErrorType ErrorType { get; private set; }
    public string? ErrorMessage { get; private set; }
    public HttpStatusCode? HttpStatusCode { get; private set; }

    private GraphQlResponse()
    {
    }

    public static GraphQlResponse Success(string content) => new()
    {
        IsSuccess = true,
        Content = content,
        ErrorType = GraphQlErrorType.None
    };

    public static GraphQlResponse ConnectionError(string message) => new()
    {
        IsSuccess = false,
        ErrorType = GraphQlErrorType.ConnectionError,
        ErrorMessage = message
    };

    public static GraphQlResponse HttpError(HttpStatusCode statusCode, string reasonPhrase, string content) => new()
    {
        IsSuccess = false,
        ErrorType = GraphQlErrorType.HttpError,
        ErrorMessage = $"HTTP {(int)statusCode} {reasonPhrase}",
        HttpStatusCode = statusCode,
        Content = content
    };

    public static GraphQlResponse GraphQlErrors(string content) => new()
    {
        IsSuccess = false,
        ErrorType = GraphQlErrorType.GraphQlErrors,
        Content = content
    };

    public static GraphQlResponse UnexpectedError(string message) => new()
    {
        IsSuccess = false,
        ErrorType = GraphQlErrorType.UnexpectedError,
        ErrorMessage = message
    };

    /// <summary>
    /// Formats the response for display to users
    /// </summary>
    public string FormatForDisplay()
    {
        if (IsSuccess)
        {
            return FormatSuccessResponse();
        }

        return ErrorType switch
        {
            GraphQlErrorType.ConnectionError => FormatConnectionError(),
            GraphQlErrorType.HttpError => FormatHttpError(),
            GraphQlErrorType.GraphQlErrors => FormatGraphQlErrors(),
            GraphQlErrorType.UnexpectedError => FormatUnexpectedError(),
            _ => "Unknown error occurred"
        };
    }

    private string FormatSuccessResponse()
    {
        try
        {
            using var document = JsonDocument.Parse(Content!);
            var formatted = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });

            var result = new StringBuilder();
            result.AppendLine("# GraphQL Query Result\n");
            result.AppendLine("✅ **Status:** Success\n");
            result.AppendLine("## Response");
            result.AppendLine("```json");
            result.AppendLine(formatted);
            result.AppendLine("```");

            return result.ToString();
        }
        catch
        {
            return $"# GraphQL Query Result\n\n✅ **Status:** Success\n\n## Response\n{Content}";
        }
    }

    private string FormatConnectionError()
    {
        var result = new StringBuilder();
        result.AppendLine("# GraphQL Connection Error\n");
        result.AppendLine("❌ **Status:** Connection Failed\n");
        result.AppendLine("## Error Details");
        result.AppendLine($"**Message:** {ErrorMessage}\n");
        result.AppendLine("## Troubleshooting Steps");
        result.AppendLine("1. **Check the endpoint URL** - Ensure it's correct and accessible");
        result.AppendLine("2. **Verify network connectivity** - Can you reach the server?");
        result.AppendLine("3. **Check firewall settings** - Are there any blocked ports?");
        result.AppendLine("4. **Validate DNS resolution** - Does the hostname resolve correctly?");
        result.AppendLine("5. **Review authentication** - Are the required headers/tokens provided?");

        return result.ToString();
    }

    private string FormatHttpError()
    {
        var result = new StringBuilder();
        result.AppendLine("# GraphQL HTTP Error\n");
        result.AppendLine($"❌ **Status:** {ErrorMessage}\n");
        result.AppendLine("## Error Details");

        if (!string.IsNullOrEmpty(Content))
        {
            try
            {
                using var document = JsonDocument.Parse(Content);
                var formatted = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
                result.AppendLine("```json");
                result.AppendLine(formatted);
                result.AppendLine("```");
            }
            catch
            {
                result.AppendLine($"```\n{Content}\n```");
            }
        }

        result.AppendLine("\n## Troubleshooting Steps");
        result.AppendLine(HttpStatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "- **Check authentication** - Verify API keys, tokens, or credentials",
            System.Net.HttpStatusCode.Forbidden => "- **Check permissions** - Ensure your account has access to this endpoint",
            System.Net.HttpStatusCode.NotFound => "- **Check endpoint URL** - Verify the GraphQL endpoint path is correct",
            System.Net.HttpStatusCode.BadRequest => "- **Check request format** - Verify query syntax and variable types",
            System.Net.HttpStatusCode.InternalServerError => "- **Server error** - Contact the API provider or check server logs",
            System.Net.HttpStatusCode.ServiceUnavailable => "- **Service unavailable** - The server may be temporarily down",
            _ => "- **Check server status** - Verify the GraphQL service is running properly"
        });

        return result.ToString();
    }

    private string FormatGraphQlErrors()
    {
        try
        {
            using var document = JsonDocument.Parse(Content!);
            var formatted = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });

            var result = new StringBuilder();
            result.AppendLine("# GraphQL Query Errors\n");
            result.AppendLine("❌ **Status:** GraphQL Errors Present\n");
            result.AppendLine("## Error Details");
            result.AppendLine("```json");
            result.AppendLine(formatted);
            result.AppendLine("```");

            return result.ToString();
        }
        catch
        {
            return $"# GraphQL Query Errors\n\n❌ **Status:** GraphQL Errors Present\n\n## Error Details\n{Content}";
        }
    }

    private string FormatUnexpectedError()
    {
        var result = new StringBuilder();
        result.AppendLine("# Unexpected Error\n");
        result.AppendLine("❌ **Status:** Unexpected Error\n");
        result.AppendLine("## Error Details");
        result.AppendLine($"**Message:** {ErrorMessage}\n");
        result.AppendLine("## Recommended Actions");
        result.AppendLine("1. **Retry the request** - This may be a temporary issue");
        result.AppendLine("2. **Check the query syntax** - Ensure valid GraphQL");
        result.AppendLine("3. **Verify endpoint configuration** - Check URL and headers");
        result.AppendLine("4. **Report the issue** - If the problem persists, report it as a bug");

        return result.ToString();
    }
}

/// <summary>
/// Types of errors that can occur during GraphQL requests
/// </summary>
public enum GraphQlErrorType
{
    None,
    ConnectionError,
    HttpError,
    GraphQlErrors,
    UnexpectedError
}