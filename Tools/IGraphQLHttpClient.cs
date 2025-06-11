using System.Net.Http;

namespace Tools;

/// <summary>
/// Interface for creating properly configured HttpClient instances for GraphQL operations
/// </summary>
public interface IGraphQLHttpClient
{
    /// <summary>
    /// Creates a configured HttpClient for GraphQL operations
    /// </summary>
    /// <param name="headers">Optional headers to add (as JSON string)</param>
    /// <param name="timeout">Optional timeout override</param>
    /// <returns>Configured HttpClient</returns>
    HttpClient CreateClient(string? headers = null, TimeSpan? timeout = null);
    
    /// <summary>
    /// Creates a configured HttpClient for GraphQL operations
    /// </summary>
    /// <param name="headers">Optional headers to add (as dictionary)</param>
    /// <param name="timeout">Optional timeout override</param>
    /// <returns>Configured HttpClient</returns>
    HttpClient CreateClient(Dictionary<string, string>? headers, TimeSpan? timeout = null);
}
