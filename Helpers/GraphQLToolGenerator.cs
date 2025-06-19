using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Graphql.Mcp.DTO;
using HotChocolate.Language;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper class for generating GraphQL tools
/// </summary>
public static class GraphQlToolGenerator
{
    /// <summary>
    /// Generates tools for a specific GraphQL type (HotChocolate version)
    /// </summary>
    public static int GenerateToolsForType(ObjectTypeDefinitionNode typeDefinition, string operationType, GraphQlEndpointInfo endpointInfo)
    {
        var toolsGenerated = 0;

        foreach (var field in typeDefinition.Fields)
        {
            try
            {
                var fieldName = field.Name.Value;
                var toolName = GenerateToolName(endpointInfo.ToolPrefix, operationType, fieldName);

                var operation = GraphQlOperationHelper.GenerateOperationString(field, operationType, fieldName);
                var description = GraphQlOperationHelper.GetFieldDescription(field, operationType, fieldName);
                var operationName = $"{operationType}_{fieldName}";

                var toolInfo = new DynamicToolInfo
                {
                    ToolName = toolName,
                    EndpointName = endpointInfo.Name,
                    OperationType = operationType,
                    OperationName = operationName,
                    Operation = operation,
                    Description = description,
                    // We'll need to convert this to JsonElement or create a new field for HotChocolate types
                    SchemaFieldDefinition = ConvertFieldToJsonElement(field)
                };

                EndpointRegistryService.Instance.RegisterDynamicTool(toolName, toolInfo);
                toolsGenerated++;
            }
            catch (Exception ex)
            {
                // Log error but continue with next field instead of failing completely
                // In a production system, you might want to use a proper logging framework
                System.Diagnostics.Debug.WriteLine($"Error generating tool for field {field.Name.Value}: {ex.Message}");
            }
        }

        return toolsGenerated;
    }

    /// <summary>
    /// Converts HotChocolate FieldDefinitionNode to JsonElement for backward compatibility
    /// </summary>
    private static JsonElement ConvertFieldToJsonElement(FieldDefinitionNode field)
    {
        // For now, return a minimal JsonElement - we might want to improve this later
        var jsonString = $$"""
        {
            "name": "{{field.Name.Value}}",
            "description": "{{field.Description?.Value ?? ""}}",
            "type": {
                "kind": "NAMED_TYPE",
                "name": "String"
            }
        }
        """;
        
        return JsonSerializer.Deserialize<JsonElement>(jsonString);
    }

    /// <summary>
    /// Generates tools for a specific GraphQL type (legacy JsonElement version)
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

            var operation = GraphQlOperationHelper.GenerateOperationString(field, operationType, fieldNameStr);
            var description = GraphQlOperationHelper.GetFieldDescription(field, operationType, fieldNameStr);
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
