using System.ComponentModel;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Tools for managing GraphQL endpoint registration and configuration
/// </summary>
[McpServerToolType]
public static class EndpointManagementTools
{
    [McpServerTool, Description(@"Register a GraphQL endpoint and automatically generate MCP tools for all available queries and mutations.

This tool performs the following operations:
1. Connects to the GraphQL endpoint
2. Introspects the schema to discover all available operations
3. Generates dynamic MCP tools for each query and mutation
4. Registers the endpoint for future operations

Schema Discovery:
- Automatically detects all Query type operations
- Optionally includes Mutation type operations (if allowMutations=true)
- Extracts type information, field descriptions, and parameters
- Generates tools with rich descriptions and type information

Authentication Support:
- Custom HTTP headers for API keys, JWT tokens, etc.
- Example headers: '{""Authorization"": ""Bearer token123"", ""X-API-Key"": ""key456""}'

Generated Tools:
- Each operation becomes an individual MCP tool
- Tools are named with pattern: [prefix_]operationType_operationName
- Include parameter validation and type information
- Provide operation-specific examples and documentation

Error Handling:
- Validates endpoint accessibility
- Checks GraphQL schema validity
- Reports connection and authentication issues")]
    public static async Task<string> RegisterEndpoint(
        [Description("GraphQL endpoint URL. Examples: 'https://api.github.com/graphql', 'http://localhost:4000/graphql'")]
        string endpoint,
        [Description("Unique identifier for this endpoint. Used to reference the endpoint in other tools. Example: 'github-api', 'local-crm'")]
        string endpointName,
        [Description("HTTP headers as JSON object for authentication. Example: '{\"Authorization\": \"Bearer token123\", \"X-API-Key\": \"key456\"}'")]
        string? headers = null,
        [Description("Whether to register mutation operations as tools. Set to true for endpoints where you want to modify data")]
        bool allowMutations = false,
        [Description("Prefix for generated tool names. Example: 'crm' generates 'crm_query_getUsers' instead of 'query_getUsers'")]
        string toolPrefix = "")
    {
        if (string.IsNullOrEmpty(endpoint))
            return "Error: GraphQL endpoint URL cannot be null or empty.";

        if (string.IsNullOrEmpty(endpointName))
            return "Error: Endpoint name cannot be null or empty.";

        try
        {
            var (requestHeaders, headerError) = JsonHelpers.ParseHeadersJson(headers);

            if (headerError != null)
                return headerError;

            var endpointInfo = new GraphQlEndpointInfo
            {
                Name = endpointName,
                Url = endpoint,
                Headers = requestHeaders,
                AllowMutations = allowMutations,
                ToolPrefix = toolPrefix
            };

            EndpointRegistryService.Instance.RegisterEndpoint(endpointName, endpointInfo);

            return await GraphQlSchemaHelper.GenerateToolsFromSchema(endpointInfo);
        }
        catch (Exception ex)
        {
            return $"Error registering endpoint: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"View all registered GraphQL endpoints with their configuration, capabilities, and generated tool counts.

Displays comprehensive information about each registered endpoint:

Endpoint Information:
- Endpoint name and URL
- Authentication headers (count, not values for security)
- Mutation support status
- Tool prefix configuration
- Number of generated dynamic tools

Tool Organization:
- Groups tools by endpoint
- Shows Query vs Mutation operation counts
- Lists tool naming conventions
- Displays endpoint status and health

Use Cases:
- Verify endpoint registration success
- Check available operations before querying
- Troubleshoot connection issues
- Audit endpoint configurations
- Plan query strategies across multiple endpoints")]
    public static string GetAllEndpoints()
    {
        var endpoints = EndpointRegistryService.Instance.GetAllEndpoints();

        if (endpoints.Count == 0)
        {
            return "No GraphQL endpoints are currently registered. Use RegisterEndpoint to add an endpoint.";
        }

        var result = new System.Text.StringBuilder();
        result.AppendLine("# Registered GraphQL Endpoints\n");

        foreach (var endpoint in endpoints)
        {
            result.AppendLine($"## {endpoint.Key}");
            result.AppendLine($"**URL:** {endpoint.Value.Url}");

            if (endpoint.Value.Headers.Count > 0)
            {
                result.AppendLine($"**Headers:** {endpoint.Value.Headers.Count} custom header(s)");
            }

            result.AppendLine($"**Allows Mutations:** {(endpoint.Value.AllowMutations ? "Yes" : "No")}");

            if (!string.IsNullOrEmpty(endpoint.Value.ToolPrefix))
            {
                result.AppendLine($"**Tool Prefix:** {endpoint.Value.ToolPrefix}");
            }

            var toolCount = EndpointRegistryService.Instance.GetToolCountForEndpoint(endpoint.Key);
            result.AppendLine($"**Dynamic Tools:** {toolCount}");
            result.AppendLine();
        }

        return result.ToString();
    }

    [McpServerTool, Description(@"Update dynamic tools for an endpoint by re-introspecting its GraphQL schema.

This tool is useful when:
- The GraphQL schema has been updated with new operations
- Field definitions or types have changed
- New mutations or queries have been added
- You want to refresh tool descriptions with latest schema information

Process:
1. Re-connects to the GraphQL endpoint
2. Performs fresh schema introspection
3. Removes old dynamic tools for this endpoint
4. Generates new tools based on current schema
5. Preserves endpoint configuration (headers, mutations setting, etc.)

Schema Changes Detected:
- New queries and mutations
- Modified field signatures
- Updated type definitions
- Changed parameter requirements
- Added or removed deprecations

Note: This operation will replace all existing dynamic tools for the specified endpoint.")]
    public static async Task<string> RefreshEndpointTools(
        [Description("Name of the registered endpoint to refresh. Use GetAllEndpoints to see available endpoints")]
        string endpointName)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Endpoint '{endpointName}' not found. Use RegisterEndpoint first.";
        }

        EndpointRegistryService.Instance.RemoveEndpoint(endpointName, out var toolsRemoved);

        var result = await GraphQlSchemaHelper.GenerateToolsFromSchema(endpointInfo);

        return $"Refreshed tools for endpoint '{endpointName}'. Removed {toolsRemoved} existing tools. {result}";
    }

    [McpServerTool, Description("Remove a GraphQL endpoint and clean up all its auto-generated dynamic tools")]
    public static string UnregisterEndpoint(
        [Description("Name of the endpoint to unregister")]
        string endpointName)
    {
        if (string.IsNullOrEmpty(endpointName))
            return "Error: Endpoint name cannot be null or empty.";

        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Endpoint '{endpointName}' not found.";
        }

        var toolsRemoved = EndpointRegistryService.Instance.RemoveToolsForEndpoint(endpointName);
        EndpointRegistryService.Instance.RemoveEndpoint(endpointName, out _);

        return $"Unregistered endpoint '{endpointName}' and removed {toolsRemoved} associated tools.";
    }
}