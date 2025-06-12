using System.Text.Json;
using Graphql.Mcp.DTO;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper class for generating GraphQL tools
/// </summary>
public static class GraphQLToolGenerator
{
    /// <summary>
    /// Generates tools for a specific GraphQL type
    /// </summary>
    public static int GenerateToolsForType(JsonElement type, string operationType, GraphQlEndpointInfo endpointInfo)
    {
        var toolsGenerated = 0;

        if (!type.TryGetProperty("fields", out var fields) || fields.ValueKind != JsonValueKind.Array)
            return 0;

        foreach (var field in fields.EnumerateArray())
        {
            if (!field.TryGetProperty("name", out var fieldName))
                continue;

            var fieldNameStr = fieldName.GetString() ?? "";
            var toolName = GenerateToolName(endpointInfo.ToolPrefix, operationType, fieldNameStr);

            var operation = GraphQLOperationHelper.GenerateOperationString(field, operationType, fieldNameStr);
            var description = GraphQLOperationHelper.GetFieldDescription(field, operationType, fieldNameStr);
            var operationName = $"{operationType}_{fieldNameStr}";

            var toolInfo = new DynamicToolInfo
            {
                ToolName = toolName,
                EndpointName = endpointInfo.Name,
                OperationType = operationType,
                OperationName = operationName,
                Operation = operation,
                Description = description,
                SchemaFieldDefinition = field
            };

            EndpointRegistryService.Instance.RegisterDynamicTool(toolName, toolInfo);
            toolsGenerated++;
        }

        return toolsGenerated;
    }

    /// <summary>
    /// Generates a tool name based on prefix, operation type, and field name
    /// </summary>
    private static string GenerateToolName(string prefix, string operationType, string fieldName)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(prefix))
            parts.Add(prefix);

        parts.Add(operationType.ToLower());
        parts.Add(ToCamelCase(fieldName));

        return string.Join("_", parts);
    }

    /// <summary>
    /// Converts a string to camel case
    /// </summary>
    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLowerInvariant(input[0]) + input[1..];
    }
}
