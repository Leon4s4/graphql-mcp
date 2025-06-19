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
    [McpServerTool, Description("Automatically generate complete GraphQL queries with intelligent field selection and configurable depth control. This tool analyzes the schema and builds optimized queries by: automatically selecting all scalar fields, intelligently handling nested object relationships, respecting circular reference protection via depth limits, including only necessary variables and parameters, generating proper field selections for return types, handling both simple and complex nested structures. Perfect for exploring APIs without manual query construction.")]
    public static async Task<string> BuildSmartQuery(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Operation name - the root field name to query (e.g. 'users', 'getUser', 'posts')")]
        string operationName,
        [Description("Maximum nesting depth to prevent infinite recursion. Default 3 levels")]
        int maxDepth = 3,
        [Description("Whether to automatically include all scalar fields (id, name, email, etc). Default true")]
        bool includeAllScalars = true,
        [Description("Variables as JSON object for parameterized queries. Example: {\"limit\": 10, \"id\": 123}")]
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

    [McpServerTool, Description("Generate nested field selections for specific GraphQL types with configurable depth limits and intelligent field traversal. This tool creates optimized field selection sets by: analyzing type relationships and dependencies, handling nested object types recursively, respecting depth limits to prevent infinite loops, including scalar fields automatically, managing list and non-null type wrappers, generating proper selection syntax. Use this for building custom query fragments or understanding type structures.")]
    public static async Task<string> BuildNestedSelection(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("GraphQL type name to build selection for (e.g. 'User', 'Post', 'Address')")]
        string typeName,
        [Description("Maximum nesting depth to prevent infinite recursion. Default 3 levels")]
        int maxDepth = 3,
        [Description("Current depth level for recursive calls. Usually leave as default 1")]
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