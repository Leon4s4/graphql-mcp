using System.ComponentModel;
using System.Text;
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
    [McpServerTool, Description("Automatically build complete GraphQL queries with smart field selection")]
    public static async Task<string> BuildSmartQuery(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Operation name (query field name)")]
        string operationName,
        [Description("Maximum nesting depth")] int maxDepth = 3,
        [Description("Include all scalar fields automatically")]
        bool includeAllScalars = true,
        [Description("Variables as JSON object (optional)")]
        string? variables = null,
        [Description("HTTP headers as JSON object (optional)")]
        string? headers = null)
    {
        try
        {
            // Get schema
            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            if (!schemaData.TryGetProperty("data", out var data) ||
                !data.TryGetProperty("__schema", out var schema))
            {
                return "Failed to retrieve schema data";
            }

            // Find the Query type
            if (!schema.TryGetProperty("queryType", out var queryTypeRef) ||
                !queryTypeRef.TryGetProperty("name", out var queryTypeName))
            {
                return "No Query type found in schema";
            }

            var queryType = FindTypeByName(schema, queryTypeName.GetString() ?? "Query");
            if (!queryType.HasValue)
            {
                return "Query type not found in schema types";
            }

            // Find the specific operation field
            if (!queryType.Value.TryGetProperty("fields", out var fields))
            {
                return "No fields found in Query type";
            }

            JsonElement? operationField = null;
            foreach (var field in fields.EnumerateArray())
            {
                if (field.TryGetProperty("name", out var nameElement) &&
                    nameElement.GetString() == operationName)
                {
                    operationField = field;
                    break;
                }
            }

            if (!operationField.HasValue)
            {
                return $"Operation '{operationName}' not found in Query type";
            }

            // Build the query
            var queryBuilder = new StringBuilder();
            var parsedVars = new Dictionary<string, object>();

            // Add variables to the operation if provided
            if (!string.IsNullOrEmpty(variables))
            {
                parsedVars = ParseVariables(variables);
                if (parsedVars.Count > 0)
                {
                    var varDefs = string.Join(", ", parsedVars.Select(kvp => $"${kvp.Key}: {kvp.Value}"));
                    queryBuilder.AppendLine($"query Get{operationName}({varDefs}) {{");
                }
                else
                {
                    queryBuilder.AppendLine($"query Get{operationName} {{");
                }
            }
            else
            {
                queryBuilder.AppendLine($"query Get{operationName} {{");
            }

            // Build field selection
            var fieldSelection = BuildOperationFieldSelection(operationField.Value, schema, maxDepth, includeAllScalars, operationName);

            queryBuilder.Append($"  {operationName}");

            // Add arguments if we have variables
            if (parsedVars.Count > 0)
            {
                var args = string.Join(", ", parsedVars.Select(kvp => $"{kvp.Key}: ${kvp.Key}"));
                queryBuilder.Append($"({args})");
            }

            queryBuilder.AppendLine(" {");
            queryBuilder.Append(fieldSelection);
            queryBuilder.AppendLine("  }");
            queryBuilder.AppendLine("}");

            var result = new StringBuilder();
            result.AppendLine("# Automatically Generated GraphQL Query\n");
            result.AppendLine("## Query");
            result.AppendLine("```graphql");
            result.AppendLine(queryBuilder.ToString());
            result.AppendLine("```\n");

            result.AppendLine("## Configuration");
            result.AppendLine($"- **Operation:** {operationName}");
            result.AppendLine($"- **Max Depth:** {maxDepth}");
            result.AppendLine($"- **Include All Scalars:** {includeAllScalars}");

            if (!string.IsNullOrEmpty(variables))
            {
                result.AppendLine($"- **Variables:** {variables}");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error building query: {ex.Message}";
        }
    }

    [McpServerTool, Description("Build nested field selections for GraphQL types with automatic scalar detection")]
    public static async Task<string> BuildNestedSelection(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Type name to build selection for")]
        string typeName,
        [Description("Maximum nesting depth")] int maxDepth = 3,
        [Description("Current depth (for recursive calls)")]
        int currentDepth = 1,
        [Description("HTTP headers as JSON object (optional)")]
        string? headers = null)
    {
        try
        {
            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            if (!schemaData.TryGetProperty("data", out var data) ||
                !data.TryGetProperty("__schema", out var schema))
            {
                return "Failed to retrieve schema data";
            }

            var type = FindTypeByName(schema, typeName);
            if (!type.HasValue)
            {
                return $"Type '{typeName}' not found in schema";
            }

            var selection = BuildNestedFieldSelection(type.Value, schema, maxDepth, currentDepth, true);

            var result = new StringBuilder();
            result.AppendLine($"# Nested Field Selection for {typeName}\n");
            result.AppendLine("## Field Selection");
            result.AppendLine("```graphql");
            result.AppendLine("{");
            result.Append(selection);
            result.AppendLine("}");
            result.AppendLine("```");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error building nested selection: {ex.Message}";
        }
    }

    private static string BuildFieldSelection(JsonElement field, JsonElement schema, int maxDepth, bool includeAllScalars, int currentDepth)
    {
        if (currentDepth > maxDepth)
        {
            return "    # Max depth reached\n";
        }

        var selection = new StringBuilder();
        var indent = new string(' ', currentDepth * 2);

        // Get the return type of this field
        if (!field.TryGetProperty("type", out var fieldType))
        {
            return "";
        }

        var typeName = GetNamedTypeName(fieldType);
        if (string.IsNullOrEmpty(typeName))
        {
            return "";
        }

        // Check if this is a scalar type
        if (IsScalarType(typeName))
        {
            // For scalar types, we don't need to recurse - just return empty since
            // the field name will be added by the caller
            return "";
        }

        // Find the type definition for object/interface types
        var typeDefinition = FindTypeByName(schema, typeName);
        if (!typeDefinition.HasValue)
        {
            return "";
        }

        // Build nested selections for the object/interface type
        var nestedSelection = BuildNestedFieldSelection(typeDefinition.Value, schema, maxDepth, currentDepth, includeAllScalars);
        return nestedSelection;
    }

    private static string BuildNestedFieldSelection(JsonElement type, JsonElement schema, int maxDepth, int currentDepth, bool includeAllScalars = true)
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

            var fieldTypeName = GetNamedTypeName(fieldType);
            var isScalar = IsScalarType(fieldTypeName);

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
                    var nestedType = FindTypeByName(schema, fieldTypeName);
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

    private static JsonElement? FindTypeByName(JsonElement schema, string typeName)
    {
        if (!schema.TryGetProperty("types", out var types))
            return null;

        foreach (var type in types.EnumerateArray())
        {
            if (type.TryGetProperty("name", out var name) &&
                name.GetString() == typeName)
            {
                return type;
            }
        }

        return null;
    }

    private static string GetNamedTypeName(JsonElement type)
    {
        // Unwrap NonNull and List types to get the actual type name
        var current = type;
        while (current.TryGetProperty("ofType", out var ofType) && ofType.ValueKind != JsonValueKind.Null)
        {
            current = ofType;
        }

        if (current.TryGetProperty("name", out var name))
        {
            return name.GetString() ?? "";
        }

        return "";
    }

    private static bool IsScalarType(string typeName)
    {
        var scalarTypes = new[]
        {
            "String", "Int", "Float", "Boolean", "ID",
            // Common custom scalars
            "DateTime", "Date", "Time", "JSON", "Upload", "Long", "Decimal"
        };
        return scalarTypes.Contains(typeName);
    }

    private static Dictionary<string, object> ParseVariables(string variables)
    {
        try
        {
            if (string.IsNullOrEmpty(variables))
                return new Dictionary<string, object>();

            using var document = JsonDocument.Parse(variables);
            return JsonHelpers.JsonElementToDictionary(document.RootElement);
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }


    private static string BuildOperationFieldSelection(JsonElement operationField, JsonElement schema, int maxDepth, bool includeAllScalars, string operationName)
    {
        // Get the return type of the operation field
        if (!operationField.TryGetProperty("type", out var fieldType))
        {
            return "    # No return type found\n";
        }

        var typeName = GetNamedTypeName(fieldType);
        if (string.IsNullOrEmpty(typeName))
        {
            return "    # Invalid return type\n";
        }

        // Check if this is a scalar type
        if (IsScalarType(typeName))
        {
            // For scalar operations, no nested selection needed
            return "";
        }

        // Find the type definition for object/interface types
        var typeDefinition = FindTypeByName(schema, typeName);
        if (!typeDefinition.HasValue)
        {
            return "    # Type definition not found\n";
        }

        // Build nested selections starting from depth 1 (inside the operation)
        var nestedSelection = BuildNestedFieldSelection(typeDefinition.Value, schema, maxDepth, 1, includeAllScalars);
        return nestedSelection;
    }
}