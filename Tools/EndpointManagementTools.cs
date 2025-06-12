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
    [McpServerTool, Description("Register a GraphQL endpoint and automatically generate MCP tools for all available queries and mutations")]
    public static async Task<string> RegisterEndpoint(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Unique name for this endpoint")]
        string endpointName,
        [Description("HTTP headers as JSON object (optional)")]
        string? headers = null,
        [Description("Allow mutations to be registered as tools")]
        bool allowMutations = false,
        [Description("Tool prefix for generated tools")]
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

    [McpServerTool, Description("View all registered GraphQL endpoints with their configuration and tool counts")]
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

    [McpServerTool, Description("Update dynamic tools for an endpoint by re-introspecting its GraphQL schema")]
    public static async Task<string> RefreshEndpointTools(
        [Description("Name of the endpoint to refresh")]
        string endpointName)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Endpoint '{endpointName}' not found. Use RegisterEndpoint first.";
        }

        var toolsRemoved = EndpointRegistryService.Instance.RemoveToolsForEndpoint(endpointName);

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