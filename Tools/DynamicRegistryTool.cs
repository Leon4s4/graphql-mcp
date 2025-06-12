using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Dynamic tool execution and listing for GraphQL operations
/// Uses EndpointRegistryService singleton to access registered endpoints and their tools
/// </summary>
[McpServerToolType]
public static class DynamicRegistryTool
{
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
}