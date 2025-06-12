using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Dynamic tool registry that generates MCP tools from GraphQL schema operations
/// Uses EndpointRegistryService singleton to persist data across MCP tool calls
/// </summary>
[McpServerToolType]
public static class DynamicRegistryTool
{
    [McpServerTool, Description("Register a GraphQL endpoint for automatic tool generation")]
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

    [McpServerTool, Description("List all registered dynamic tools")]
    public static string ListDynamicTools()
    {
        var tools = EndpointRegistryService.Instance.GetAllDynamicTools().Values.ToList();

        if (tools.Count == 0)
            return "No dynamic tools are currently registered. Use RegisterEndpoint to generate tools from a GraphQL schema.";

        var result = new StringBuilder();
        result.AppendLine("# Registered Dynamic Tools");
        result.AppendLine();

        var endpointGroups = tools.GroupBy(t => t.EndpointName)
            .ToList();

        foreach (var group in endpointGroups)
        {
            var endpoint = EndpointRegistryService.Instance.GetEndpointInfo(group.Key);
            if (endpoint is null) continue;

            result.AppendLine($"## Endpoint: {group.Key}");
            result.AppendLine($"**URL:** {endpoint.Url}");
            result.AppendLine($"**Operations:** {group.Count()}");
            result.AppendLine();

            var queries = group.Where(t => t.OperationType == "Query").ToList();
            result.Append(MarkdownFormatHelpers.FormatToolSection("Queries", queries));

            var mutations = group.Where(t => t.OperationType == "Mutation").ToList();
            result.Append(MarkdownFormatHelpers.FormatToolSection("Mutations", mutations));
        }

        return result.ToString();
    }

    [McpServerTool, Description("Execute a dynamically generated GraphQL operation")]
    public static async Task<string> ExecuteDynamicOperation(
        [Description("Name of the dynamic tool to execute")]
        string toolName,
        [Description("Variables for the operation as JSON object")]
        string? variables = null)
    {
        try
        {
            var toolInfo = EndpointRegistryService.Instance.GetDynamicTool(toolName);
            if (toolInfo == null)
                return $"Dynamic tool '{toolName}' not found. Use ListDynamicTools to see available tools.";

            var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(toolInfo.EndpointName);
            if (endpointInfo == null)
                return $"Endpoint '{toolInfo.EndpointName}' not found for tool '{toolName}'.";

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
                query = toolInfo.Operation,
                variables = variableDict.Count > 0 ? variableDict : null,
                operationName = toolInfo.OperationName
            };

            var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(
                endpointInfo.Url,
                request,
                endpointInfo.Headers);

            return !result.IsSuccess ? result.FormatForDisplay() : GraphQLOperationHelper.FormatGraphQlResponse(result.Content!);
        }
        catch (Exception ex)
        {
            return $"Error executing dynamic operation: {ex.Message}";
        }
    }

    [McpServerTool, Description("Refresh tools for a registered endpoint (re-introspect schema)")]
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

    [McpServerTool, Description("Remove all dynamic tools for an endpoint")]
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