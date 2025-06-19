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
    [McpServerTool, Description(@"View all auto-generated GraphQL operation tools organized by endpoint with comprehensive operation details.

This tool provides a complete overview of all available GraphQL operations across registered endpoints:

Operation Information:
- Query operations for data retrieval
- Mutation operations for data modification
- Operation names and descriptions
- Parameter requirements and types
- Return type information

Tool Organization:
- Grouped by GraphQL endpoint
- Separated by operation type (Query vs Mutation)
- Shows tool naming conventions
- Displays parameter signatures
- Includes operation complexity hints

Use Cases:
- Discover available GraphQL operations
- Find the correct tool name for specific operations
- Understand parameter requirements before execution
- Browse API capabilities across multiple endpoints
- Plan complex query strategies

Tool Naming Convention:
- Pattern: [prefix_]operationType_operationName
- Examples: 'query_getUsers', 'crm_mutation_createUser'
- Prefix helps organize tools from multiple endpoints")]
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

    [McpServerTool, Description("Execute a specific auto-generated GraphQL operation with type-safe variable validation and rich error handling. This tool provides targeted execution of specific GraphQL operations with pre-built optimized queries, validates variables against schema types, handles operation-specific error cases, and returns formatted structured results. Use ListDynamicTools to see all available tools and their names.")]
    public static async Task<string> ExecuteDynamicOperation(
        [Description("Name of the dynamic tool to execute. Use ListDynamicTools to see all available tools and their names")]
        string toolName,
        [Description("Variables for the operation as JSON object. Must match the operation's parameter schema. Example: {\"id\": 123, \"input\": {\"name\": \"John\"}}")]
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

            var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, request);

            return !result.IsSuccess ? result.FormatForDisplay() : GraphQlOperationHelper.FormatGraphQlResponse(result.Content!);
        }
        catch (Exception ex)
        {
            return $"Error executing dynamic operation: {ex.Message}";
        }
    }
}