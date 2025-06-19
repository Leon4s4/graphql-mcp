using System.Text;
using System.Text.Json;
using HotChocolate.Language;
using System.Linq;
using System.Collections.Generic;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper class for GraphQL operation generation and formatting
/// </summary>
public static class GraphQlOperationHelper
{
    /// <summary>
    /// Generates a GraphQL operation string (HotChocolate version) with enhanced field selection
    /// </summary>
    public static string GenerateOperationString(FieldDefinitionNode field, string operationType, string fieldName)
    {
        var operation = new StringBuilder();

        operation.AppendLine($"{operationType.ToLower()} {operationType}_{fieldName}(");

        if (field.Arguments.Any())
        {
            var parameters = new List<string>();
            foreach (var arg in field.Arguments)
            {
                var paramName = arg.Name.Value;
                var paramType = GetTypeName(arg.Type);
                parameters.Add($"${paramName}: {paramType}");
            }

            if (parameters.Count > 0)
            {
                operation.AppendLine(string.Join(",\n  ", parameters));
            }
        }

        operation.AppendLine(") {");
        operation.AppendLine($"  {fieldName}");

        // Add arguments if any
        if (field.Arguments.Any())
        {
            operation.Append("(");
            var argList = field.Arguments.Select(arg => $"{arg.Name.Value}: ${arg.Name.Value}").ToList();
            operation.Append(string.Join(", ", argList));
            operation.Append(")");
        }

        // Enhanced field selection based on return type
        var returnTypeName = GetTypeName(field.Type);
        operation.AppendLine(" {");
        
        // Add some common scalar fields that are likely to exist
        operation.AppendLine("    # Common fields - customize based on your needs:");
        operation.AppendLine("    # id");
        operation.AppendLine("    # name");
        operation.AppendLine("    # title");
        operation.AppendLine("    # createdAt");
        operation.AppendLine("    # updatedAt");
        operation.AppendLine("    ");
        operation.AppendLine("    # Use BuildSmartQuery tool for automatic field selection");
        operation.AppendLine("    # Use GetSchema tool to see available fields");
        
        operation.AppendLine("  }");
        operation.AppendLine("}");

        return operation.ToString();
    }

    /// <summary>
    /// Gets field description (HotChocolate version)
    /// </summary>
    public static string GetFieldDescription(FieldDefinitionNode field, string operationType, string fieldName)
    {
        var description = new StringBuilder();
        
        // Start with operation type and name
        description.AppendLine($"Execute {operationType.ToLower()} operation: {fieldName}");
        
        // Add field description if available
        if (!string.IsNullOrEmpty(field.Description?.Value))
        {
            description.AppendLine();
            description.AppendLine($"Description: {field.Description.Value}");
        }
        
        // Add parameters information
        if (field.Arguments.Any())
        {
            description.AppendLine();
            description.AppendLine("Parameters:");
            foreach (var arg in field.Arguments)
            {
                var argType = GetTypeName(arg.Type);
                var isRequired = arg.Type is NonNullTypeNode;
                var requiredMarker = isRequired ? " (REQUIRED)" : " (optional)";
                
                description.Append($"- {arg.Name.Value}: {argType}{requiredMarker}");
                
                if (!string.IsNullOrEmpty(arg.Description?.Value))
                {
                    description.Append($" - {arg.Description.Value}");
                }
                description.AppendLine();
            }
        }
        
        // Add return type information
        var returnType = GetTypeName(field.Type);
        description.AppendLine();
        description.AppendLine($"Returns: {returnType}");
        
        // Add usage examples
        description.AppendLine();
        description.AppendLine("Usage Examples:");
        if (field.Arguments.Any())
        {
            description.AppendLine($"- With variables: Use ExecuteDynamicOperation with toolName '{operationType.ToLower()}_{fieldName}'");
            description.AppendLine($"- Direct query: Use QueryGraphQl with operation string");
        }
        else
        {
            description.AppendLine($"- Simple execution: Use ExecuteDynamicOperation with toolName '{operationType.ToLower()}_{fieldName}'");
        }
        
        return description.ToString();
    }

    /// <summary>
    /// Helper method to get type name from HotChocolate type node
    /// </summary>
    private static string GetTypeName(ITypeNode type)
    {
        return type switch
        {
            NonNullTypeNode nonNull => GetTypeName(nonNull.Type) + "!",
            ListTypeNode list => "[" + GetTypeName(list.Type) + "]",
            NamedTypeNode named => named.Name.Value,
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Generates a GraphQL operation string (legacy JsonElement version) with enhanced guidance
    /// </summary>
    public static string GenerateOperationString(JsonElement field, string operationType, string fieldName)
    {
        var operation = new StringBuilder();

        operation.AppendLine($"{operationType.ToLower()} {operationType}_{fieldName}(");

        if (field.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Array)
        {
            var parameters = new List<string>();
            foreach (var arg in args.EnumerateArray())
            {
                if (arg.TryGetProperty("name", out var argName) &&
                    arg.TryGetProperty("type", out var argType))
                {
                    var paramName = argName.GetString() ?? "";
                    var paramType = GraphQlTypeHelpers.GetTypeName(argType);
                    parameters.Add($"${paramName}: {paramType}");
                }
            }

            if (parameters.Count > 0)
            {
                operation.AppendLine(string.Join(",\n  ", parameters));
            }
        }

        operation.AppendLine(") {");

        operation.Append($"  {fieldName}");

        if (field.TryGetProperty("args", out var fieldArgs) && fieldArgs.ValueKind == JsonValueKind.Array)
        {
            var argList = new List<string>();
            foreach (var arg in fieldArgs.EnumerateArray())
            {
                if (arg.TryGetProperty("name", out var argName))
                {
                    var paramName = argName.GetString() ?? "";
                    argList.Add($"{paramName}: ${paramName}");
                }
            }

            if (argList.Count > 0)
            {
                operation.Append($"({string.Join(", ", argList)})");
            }
        }

        operation.AppendLine(" {");
        operation.AppendLine("    # Common fields - customize based on your needs:");
        operation.AppendLine("    # id");
        operation.AppendLine("    # name");
        operation.AppendLine("    # title");
        operation.AppendLine("    # createdAt");
        operation.AppendLine("    # updatedAt");
        operation.AppendLine("    ");
        operation.AppendLine("    # Use BuildSmartQuery tool for automatic field selection");
        operation.AppendLine("    # Use GetSchema tool to see available fields for this type");
        operation.AppendLine("  }");
        operation.AppendLine("}");

        return operation.ToString();
    }

    /// <summary>
    /// Gets a description for a GraphQL field (enhanced version)
    /// </summary>
    public static string GetFieldDescription(JsonElement field, string operationType, string fieldName)
    {
        var description = new StringBuilder();
        
        // Start with operation type and name
        description.AppendLine($"Execute {operationType.ToLower()} operation: {fieldName}");
        
        // Add field description if available
        if (field.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
        {
            var descText = desc.GetString();
            if (!string.IsNullOrEmpty(descText))
            {
                description.AppendLine();
                description.AppendLine($"Description: {descText}");
            }
        }
        
        // Add parameters information
        if (field.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Array)
        {
            var argCount = args.GetArrayLength();
            if (argCount > 0)
            {
                description.AppendLine();
                description.AppendLine("Parameters:");
                
                foreach (var arg in args.EnumerateArray())
                {
                    if (arg.TryGetProperty("name", out var argName) && 
                        arg.TryGetProperty("type", out var argType))
                    {
                        var paramName = argName.GetString() ?? "";
                        var paramType = GraphQlTypeHelpers.GetTypeName(argType);
                        var isRequired = argType.TryGetProperty("kind", out var kindElement) && 
                                       kindElement.GetString() == "NON_NULL";
                        var requiredMarker = isRequired ? " (REQUIRED)" : " (optional)";
                        
                        description.Append($"- {paramName}: {paramType}{requiredMarker}");
                        
                        if (arg.TryGetProperty("description", out var argDesc) && 
                            argDesc.ValueKind == JsonValueKind.String)
                        {
                            var argDescText = argDesc.GetString();
                            if (!string.IsNullOrEmpty(argDescText))
                            {
                                description.Append($" - {argDescText}");
                            }
                        }
                        description.AppendLine();
                    }
                }
            }
        }
        
        // Add return type information
        if (field.TryGetProperty("type", out var returnType))
        {
            var returnTypeName = GraphQlTypeHelpers.GetTypeName(returnType);
            description.AppendLine();
            description.AppendLine($"Returns: {returnTypeName}");
        }
        
        // Add usage examples
        description.AppendLine();
        description.AppendLine("Usage Examples:");
        if (field.TryGetProperty("args", out var fieldArgs) && fieldArgs.ValueKind == JsonValueKind.Array && fieldArgs.GetArrayLength() > 0)
        {
            description.AppendLine($"- With variables: Use ExecuteDynamicOperation with toolName '{operationType.ToLower()}_{fieldName}'");
            description.AppendLine($"- Direct query: Use QueryGraphQl with operation string");
        }
        else
        {
            description.AppendLine($"- Simple execution: Use ExecuteDynamicOperation with toolName '{operationType.ToLower()}_{fieldName}'");
        }

        return description.ToString();
    }

    /// <summary>
    /// Formats a GraphQL response for display
    /// </summary>
    public static string FormatGraphQlResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return responseContent;
        }
    }

    /// <summary>
    /// Builds a complete GraphQL query with variables and field selection
    /// </summary>
    public static string BuildGraphQlQuery(JsonElement operationField, JsonElement schema, string operationName, 
        int maxDepth, bool includeAllScalars, Dictionary<string, object> parsedVariables)
    {
        var queryBuilder = new StringBuilder();
        
        // Build query header with variables
        AppendQueryHeader(queryBuilder, operationName, parsedVariables);
        
        // Build operation with arguments and field selection
        AppendOperationBody(queryBuilder, operationField, schema, operationName, maxDepth, includeAllScalars, parsedVariables);
        
        queryBuilder.AppendLine("}");
        return queryBuilder.ToString();
    }

    /// <summary>
    /// Appends the query header with variable definitions
    /// </summary>
    public static void AppendQueryHeader(StringBuilder queryBuilder, string operationName, Dictionary<string, object> parsedVariables)
    {
        if (parsedVariables.Count > 0)
        {
            var varDefs = string.Join(", ", parsedVariables.Select(kvp => $"${kvp.Key}: {kvp.Value}"));
            queryBuilder.AppendLine($"query Get{operationName}({varDefs}) {{");
        }
        else
        {
            queryBuilder.AppendLine($"query Get{operationName} {{");
        }
    }

    /// <summary>
    /// Appends the operation body with arguments and field selection
    /// </summary>
    public static void AppendOperationBody(StringBuilder queryBuilder, JsonElement operationField, JsonElement schema,
        string operationName, int maxDepth, bool includeAllScalars, Dictionary<string, object> parsedVariables)
    {
        var fieldSelection = BuildOperationFieldSelection(operationField, schema, maxDepth, includeAllScalars, operationName);
        
        queryBuilder.Append($"  {operationName}");

        // Add arguments if we have variables
        if (parsedVariables.Count > 0)
        {
            var args = string.Join(", ", parsedVariables.Select(kvp => $"{kvp.Key}: ${kvp.Key}"));
            queryBuilder.Append($"({args})");
        }

        queryBuilder.AppendLine(" {");
        queryBuilder.Append(fieldSelection);
        queryBuilder.AppendLine("  }");
    }

    /// <summary>
    /// Builds field selection for an operation
    /// </summary>
    public static string BuildOperationFieldSelection(JsonElement operationField, JsonElement schema, int maxDepth, bool includeAllScalars, string operationName)
    {
        // Get the return type of the operation field
        if (!operationField.TryGetProperty("type", out var fieldType))
        {
            return "    # No return type found\n";
        }

        var typeName = GraphQlTypeHelpers.GetNamedTypeName(fieldType);
        if (string.IsNullOrEmpty(typeName))
        {
            return "    # Invalid return type\n";
        }

        // Check if this is a scalar type
        if (GraphQlTypeHelpers.IsScalarType(typeName))
        {
            // For scalar operations, no nested selection needed
            return "";
        }

        // Find the type definition for object/interface types
        var typeDefinition = GraphQlTypeHelpers.FindTypeByName(schema, typeName);
        if (!typeDefinition.HasValue)
        {
            return "    # Type definition not found\n";
        }

        // Build nested selections starting from depth 1 (inside the operation)
        var nestedSelection = BuildNestedFieldSelection(typeDefinition.Value, schema, maxDepth, 1, includeAllScalars);
        return nestedSelection;
    }

    /// <summary>
    /// Builds nested field selection for GraphQL types
    /// </summary>
    public static string BuildNestedFieldSelection(JsonElement type, JsonElement schema, int maxDepth, int currentDepth, bool includeAllScalars = true)
    {
        if (currentDepth > maxDepth)
        {
            return "";
        }

        var selection = new StringBuilder();
        var indent = new string(' ', currentDepth * 2);

        if (!type.TryGetProperty("kind", out var kindElement))
        {
            return "";
        }

        var kind = kindElement.GetString();

        // Only process OBJECT and INTERFACE types
        if (kind != "OBJECT" && kind != "INTERFACE")
        {
            return "";
        }

        if (!type.TryGetProperty("fields", out var fields))
        {
            return "";
        }

        foreach (var field in fields.EnumerateArray())
        {
            if (!field.TryGetProperty("name", out var nameElement))
                continue;

            var fieldName = nameElement.GetString();
            if (fieldName?.StartsWith("__") == true)
                continue;

            if (!field.TryGetProperty("type", out var fieldType))
                continue;

            var fieldTypeName = GraphQlTypeHelpers.GetNamedTypeName(fieldType);
            var isScalar = GraphQlTypeHelpers.IsScalarType(fieldTypeName);

            if (isScalar)
            {
                // Only include scalar fields if the flag is set
                if (includeAllScalars)
                {
                    selection.AppendLine($"{indent}{fieldName}");
                }
            }
            else
            {
                // This is an object type, recurse if we haven't hit max depth
                if (currentDepth < maxDepth)
                {
                    var nestedType = GraphQlTypeHelpers.FindTypeByName(schema, fieldTypeName);
                    if (nestedType.HasValue)
                    {
                        var nestedSelection = BuildNestedFieldSelection(nestedType.Value, schema, maxDepth, currentDepth + 1, includeAllScalars);
                        if (!string.IsNullOrEmpty(nestedSelection))
                        {
                            selection.AppendLine($"{indent}{fieldName} {{");
                            selection.Append(nestedSelection);
                            selection.AppendLine($"{indent}}}");
                        }
                    }
                }
            }
        }

        return selection.ToString();
    }
}
