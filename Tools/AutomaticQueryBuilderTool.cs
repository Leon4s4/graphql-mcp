using System.ComponentModel;
using System.Text.Json;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Automatic GraphQL query builder with smart field selection and depth control
/// Equivalent to Python's build_nested_selection and build_selection functionality
/// </summary>
[McpServerToolType]
public static class AutomaticQueryBuilderTool
{
    [McpServerTool, Description("Automatically generate complete GraphQL queries with intelligent field selection and depth control")]
    public static async Task<string> BuildSmartQuery(
        [Description("Name of the registered GraphQL endpoint")] string endpointName,
        [Description("Operation name (query field name)")]
        string operationName,
        [Description("Maximum nesting depth")] int maxDepth = 3,
        [Description("Include all scalar fields automatically")]
        bool includeAllScalars = true,
        [Description("Variables as JSON object (optional)")]
        string? variables = null)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        try
        {
            var schema = await GraphQlSchemaHelper.GetSchemaAsync(endpointInfo);
            var operationField = GraphQlSchemaHelper.FindOperationField(schema, operationName);
            var parsedVariables = JsonHelpers.ParseVariables(variables ?? string.Empty);
            var query = GraphQLOperationHelper.BuildGraphQLQuery(operationField, schema, operationName, maxDepth, includeAllScalars, parsedVariables);
            
            return MarkdownFormatHelpers.FormatQueryResult(query, operationName, maxDepth, includeAllScalars, variables);
        }
        catch (Exception ex)
        {
            return $"Error building query: {ex.Message}";
        }
    }

    [McpServerTool, Description("Generate nested field selections for specific GraphQL types with configurable depth limits")]
    public static async Task<string> BuildNestedSelection(
        [Description("Name of the registered GraphQL endpoint")] string endpointName,
        [Description("Type name to build selection for")]
        string typeName,
        [Description("Maximum nesting depth")] int maxDepth = 3,
        [Description("Current depth (for recursive calls)")]
        int currentDepth = 1)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        try
        {
            var schema = await GraphQlSchemaHelper.GetSchemaAsync(endpointInfo);
            var type = GraphQlTypeHelpers.FindTypeByName(schema, typeName);
            
            if (!type.HasValue)
                return $"Type '{typeName}' not found in schema";

            var selection = GraphQLOperationHelper.BuildNestedFieldSelection(type.Value, schema, maxDepth, currentDepth, true);
            return MarkdownFormatHelpers.FormatNestedSelectionResult(selection, typeName);
        }
        catch (Exception ex)
        {
            return $"Error building nested selection: {ex.Message}";
        }
    }
}