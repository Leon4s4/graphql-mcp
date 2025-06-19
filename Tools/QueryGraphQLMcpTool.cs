using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// MCP tool for executing GraphQL queries against registered endpoints
/// </summary>
[McpServerToolType]
public static class QueryGraphQlMcpTool
{
    [McpServerTool, Description(@"Execute GraphQL queries and mutations with comprehensive analysis, suggestions, and metadata in a single response.

This enhanced tool provides intelligent GraphQL execution with smart defaults including:
- Query execution with detailed error analysis and suggestions
- Performance metrics and optimization recommendations  
- Related query suggestions based on current operation
- Schema context for referenced types and available fields
- Security analysis and permission requirements
- Pagination hints and data freshness indicators
- Alternative approaches and best practices
- Caching recommendations and complexity analysis

The response includes everything needed to understand the results, optimize performance, and discover related operations, minimizing the need for additional API calls.

Examples:
- Query: 'query { users { id name email } }' 
- Mutation: 'mutation($input: UserInput!) { createUser(input: $input) { id name } }'
- With variables: '{""name"": ""John"", ""email"": ""john@example.com""}'

Advanced Features:
- Automatic query complexity analysis and recommendations
- Context-aware field suggestions and optimizations  
- Security scanning for sensitive data exposure
- Performance profiling with execution metrics")]
    public static async Task<string> QueryGraphQLComprehensive(
        [Description("GraphQL query or mutation string. Examples: 'query { users { id name } }' or 'mutation($input: UserInput!) { createUser(input: $input) { id } }'")]
        string query,
        [Description("Name of the registered GraphQL endpoint. Use ListDynamicTools or GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Variables for the query as JSON object. Example: '{\"id\": 123, \"input\": {\"name\": \"John\", \"email\": \"john@example.com\"}}'")]
        string? variables = null,
        [Description("Include intelligent suggestions and related operations")]
        bool includeSuggestions = true,
        [Description("Include performance metrics and optimization recommendations")]
        bool includeMetrics = true,
        [Description("Include schema context for referenced types")]
        bool includeSchemaContext = true)
    {
        try
        {
            // Check if endpoint is registered using EndpointRegistryService
            if (!EndpointRegistryService.Instance.IsEndpointRegistered(endpointName))
            {
                var registeredEndpoints = EndpointRegistryService.Instance.GetRegisteredEndpointNames();
                return CreateErrorResponse("Endpoint Not Found",
                    $"Endpoint '{endpointName}' not found",
                    $"Available endpoints: {string.Join(", ", registeredEndpoints)}",
                    ["Use RegisterEndpoint to add new endpoints", "Check endpoint name spelling", "Use GetAllEndpoints to list available endpoints"]);
            }

            // Get endpoint information using EndpointRegistryService
            var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
            if (endpointInfo == null)
            {
                return CreateErrorResponse("Configuration Error",
                    "Could not retrieve endpoint information",
                    "Endpoint configuration is invalid or corrupted",
                    ["Re-register the endpoint", "Check endpoint configuration", "Contact administrator"]);
            }

            // Check if it's a mutation and if mutations are allowed
            if (IsMutation(query) && !endpointInfo.AllowMutations)
            {
                return CreateErrorResponse("Mutation Not Allowed",
                    $"Mutations are not allowed for endpoint '{endpointName}'",
                    "This endpoint is configured to only allow query operations",
                    ["Enable mutations when registering the endpoint", "Use query operations instead", "Contact administrator to enable mutations"]);
            }

            // Parse variables with enhanced error handling
            var variableDict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(variables))
            {
                try
                {
                    using var document = JsonDocument.Parse(variables);
                    variableDict = JsonHelpers.JsonElementToDictionary(document.RootElement);
                }
                catch (JsonException ex)
                {
                    return CreateErrorResponse("Variable Parsing Error",
                        $"Error parsing variables JSON: {ex.Message}",
                        "The variables parameter must be valid JSON",
                        ["Check JSON syntax for variables", "Ensure proper quotes and brackets", "Validate variable types match schema"]);
                }
            }

            var request = new
            {
                query = query,
                variables = variableDict.Count > 0 ? variableDict : null
            };

            // Execute the GraphQL request
            var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, request);

            // Parse the result to extract data and errors
            var (data, errors) = ParseGraphQLResult(result);

            // Use smart response service for comprehensive response
            var smartResponseService = GetSmartResponseService();
            return await smartResponseService.CreateExecutionResponseAsync(
                query, data, errors, variableDict, includeSuggestions, includeMetrics, includeSchemaContext);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Execution Error",
                $"Error executing GraphQL query: {ex.Message}",
                "An unexpected error occurred during query execution",
                ["Check query syntax", "Verify endpoint connectivity", "Review variables format", "Contact support if issue persists"]);
        }
    }

    /// <summary>
    /// Original QueryGraphQL method for backward compatibility
    /// </summary>
    [McpServerTool, Description("Execute basic GraphQL queries and mutations for backward compatibility. For comprehensive responses with analysis and suggestions, use QueryGraphQLComprehensive instead.")]
    public static async Task<string> QueryGraphQl(
        [Description("GraphQL query or mutation string")]
        string query,
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Variables for the query as JSON object")]
        string? variables = null)
    {
        try
        {
            // Check if endpoint is registered using EndpointRegistryService
            if (!EndpointRegistryService.Instance.IsEndpointRegistered(endpointName))
            {
                var registeredEndpoints = EndpointRegistryService.Instance.GetRegisteredEndpointNames();
                return $"Endpoint '{endpointName}' not found. Available endpoints: {string.Join(", ", registeredEndpoints)}. Use RegisterEndpoint to add new endpoints.";
            }

            // Get endpoint information using EndpointRegistryService
            var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
            if (endpointInfo == null)
            {
                return "Error: Could not retrieve endpoint information.";
            }

            // Check if it's a mutation and if mutations are allowed
            if (IsMutation(query) && !endpointInfo.AllowMutations)
            {
                return $"Mutations are not allowed for endpoint '{endpointName}'. Enable mutations when registering the endpoint to use mutation operations.";
            }

            // Parse variables
            var variableDict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(variables))
            {
                try
                {
                    using var document = JsonDocument.Parse(variables);
                    variableDict = JsonHelpers.JsonElementToDictionary(document.RootElement);
                }
                catch (JsonException ex)
                {
                    return $"Error parsing variables JSON: {ex.Message}";
                }
            }

            var request = new
            {
                query = query,
                variables = variableDict.Count > 0 ? variableDict : null
            };

            // Use centralized HTTP execution with proper error handling
            var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, request);
            return result.FormatForDisplay();
        }
        catch (Exception ex)
        {
            return $"Error executing GraphQL query: {ex.Message}";
        }
    }

    private static bool IsMutation(string query)
    {
        return Regex.IsMatch(query, @"\bmutation\b",
            RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Helper method to get or create SmartResponseService instance
    /// </summary>
    private static SmartResponseService GetSmartResponseService()
    {
        // For now, create a simple instance. In a real DI scenario, this would be injected
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = NullLogger<SmartResponseService>.Instance;
        return new SmartResponseService(cache, logger);
    }

    /// <summary>
    /// Creates a comprehensive error response with suggestions
    /// </summary>
    private static string CreateErrorResponse(string title, string message, string details, List<string> suggestions)
    {
        var errorResponse = new
        {
            error = new
            {
                title = title,
                message = message,
                details = details,
                timestamp = DateTime.UtcNow,
                suggestions = suggestions,
                type = "GRAPHQL_EXECUTION_ERROR"
            },
            metadata = new
            {
                operation = "graphql_execution",
                success = false,
                executionTimeMs = 0
            }
        };

        return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Parse GraphQL result to extract data and errors
    /// </summary>
    private static (object? data, List<ExecutionError>? errors) ParseGraphQLResult(GraphQlResponse result)
    {
        if (!result.IsSuccess || string.IsNullOrEmpty(result.Content))
        {
            return (null, [
                new ExecutionError
                {
                    Message = result.Content ?? "Unknown error",
                    Suggestions = ["Check endpoint connectivity", "Verify query syntax"]
                }
            ]);
        }

        try
        {
            using var doc = JsonDocument.Parse(result.Content);
            var root = doc.RootElement;

            object? data = null;
            List<ExecutionError>? errors = null;

            if (root.TryGetProperty("data", out var dataElement))
            {
                data = JsonHelpers.JsonElementToObject(dataElement);
            }

            if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Array)
            {
                errors = [];
                foreach (var errorElement in errorsElement.EnumerateArray())
                {
                    var error = new ExecutionError();

                    if (errorElement.TryGetProperty("message", out var messageElement))
                        error.Message = messageElement.GetString() ?? "";

                    if (errorElement.TryGetProperty("path", out var pathElement) && pathElement.ValueKind == JsonValueKind.Array)
                    {
                        error.Path = pathElement.EnumerateArray()
                            .Select<JsonElement, object>(p =>
                                p.ValueKind == JsonValueKind.String ? p.GetString()! :
                                p.ValueKind == JsonValueKind.Number ? p.GetInt32() : p.ToString()!)
                            .ToList();
                    }

                    if (errorElement.TryGetProperty("extensions", out var extensionsElement))
                    {
                        error.Extensions = JsonHelpers.JsonElementToDictionary(extensionsElement);
                    }

                    errors.Add(error);
                }
            }

            return (data, errors);
        }
        catch (Exception ex)
        {
            return (null, [
                new ExecutionError
                {
                    Message = $"Failed to parse response: {ex.Message}",
                    Suggestions = ["Check response format", "Verify endpoint returns valid GraphQL"]
                }
            ]);
        }
    }
}