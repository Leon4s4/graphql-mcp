using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace Tools;

[McpServerToolType]
public static class GraphQLSchemaTools
{
    [McpServerTool, Description("Get a focused view of the GraphQL schema with specific type information")]
    public static async Task<string> GetSchema(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Type name to focus on (optional)")] string? typeName = null,
        [Description("Include only query types")] bool queryOnly = false,
        [Description("Include only mutation types")] bool mutationOnly = false,
        [Description("HTTP headers as JSON object (optional)")] string? headers = null)
    {
        try
        {
            // Get full schema introspection
            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            if (!schemaData.TryGetProperty("data", out var data) || 
                !data.TryGetProperty("__schema", out var schema))
            {
                return "Failed to retrieve schema data";
            }

            var result = new StringBuilder();
            result.AppendLine("# GraphQL Schema\n");

            // Get root types
            var queryType = schema.TryGetProperty("queryType", out var qt) ? qt.GetProperty("name").GetString() : null;
            var mutationType = schema.TryGetProperty("mutationType", out var mt) ? mt.GetProperty("name").GetString() : null;
            var subscriptionType = schema.TryGetProperty("subscriptionType", out var st) ? st.GetProperty("name").GetString() : null;

            result.AppendLine("## Root Types");
            result.AppendLine($"- **Query:** {queryType ?? "None"}");
            result.AppendLine($"- **Mutation:** {mutationType ?? "None"}");
            result.AppendLine($"- **Subscription:** {subscriptionType ?? "None"}\n");

            if (!schema.TryGetProperty("types", out var types))
            {
                return result.ToString() + "No types found in schema";
            }

            // Filter types based on parameters
            var filteredTypes = new List<JsonElement>();
            foreach (var type in types.EnumerateArray())
            {
                if (!type.TryGetProperty("name", out var nameElement))
                    continue;

                var currentTypeName = nameElement.GetString();
                if (currentTypeName?.StartsWith("__") == true)
                    continue;

                // Apply filters
                if (!string.IsNullOrEmpty(typeName) && 
                    !currentTypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (queryOnly && currentTypeName != queryType)
                    continue;

                if (mutationOnly && currentTypeName != mutationType)
                    continue;

                filteredTypes.Add(type);
            }

            // Display types
            result.AppendLine("## Types\n");
            foreach (var type in filteredTypes)
            {
                result.AppendLine(FormatTypeDefinition(type));
                result.AppendLine();
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving schema: {ex.Message}";
        }
    }

    [McpServerTool, Description("Compare two GraphQL schemas and show differences")]
    public static async Task<string> CompareSchemas(
        [Description("First GraphQL endpoint URL")] string endpoint1,
        [Description("Second GraphQL endpoint URL")] string endpoint2,
        [Description("HTTP headers for first endpoint as JSON (optional)")] string? headers1 = null,
        [Description("HTTP headers for second endpoint as JSON (optional)")] string? headers2 = null)
    {
        try
        {
            // Get both schemas
            var schema1Json = await SchemaIntrospectionTools.IntrospectSchema(endpoint1, headers1);
            var schema2Json = await SchemaIntrospectionTools.IntrospectSchema(endpoint2, headers2);

            var schema1Data = JsonSerializer.Deserialize<JsonElement>(schema1Json);
            var schema2Data = JsonSerializer.Deserialize<JsonElement>(schema2Json);

            if (!schema1Data.TryGetProperty("data", out var data1) || 
                !data1.TryGetProperty("__schema", out var schema1) ||
                !schema2Data.TryGetProperty("data", out var data2) || 
                !data2.TryGetProperty("__schema", out var schema2))
            {
                return "Failed to retrieve schema data from one or both endpoints";
            }

            var result = new StringBuilder();
            result.AppendLine("# Schema Comparison Report\n");
            result.AppendLine($"**Schema 1:** {endpoint1}");
            result.AppendLine($"**Schema 2:** {endpoint2}\n");

            // Compare types
            var types1 = GetTypeNames(schema1);
            var types2 = GetTypeNames(schema2);

            var addedTypes = types2.Except(types1).ToList();
            var removedTypes = types1.Except(types2).ToList();
            var commonTypes = types1.Intersect(types2).ToList();

            result.AppendLine("## Type Changes\n");

            if (addedTypes.Any())
            {
                result.AppendLine("### Added Types");
                foreach (var type in addedTypes)
                {
                    result.AppendLine($"+ {type}");
                }
                result.AppendLine();
            }

            if (removedTypes.Any())
            {
                result.AppendLine("### Removed Types");
                foreach (var type in removedTypes)
                {
                    result.AppendLine($"- {type}");
                }
                result.AppendLine();
            }

            result.AppendLine($"### Summary");
            result.AppendLine($"- **Common types:** {commonTypes.Count}");
            result.AppendLine($"- **Added types:** {addedTypes.Count}");
            result.AppendLine($"- **Removed types:** {removedTypes.Count}");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error comparing schemas: {ex.Message}";
        }
    }

    private static string FormatTypeDefinition(JsonElement type)
    {
        var name = type.GetProperty("name").GetString();
        var kind = type.GetProperty("kind").GetString();
        var description = type.TryGetProperty("description", out var desc) ? desc.GetString() : null;

        var result = new StringBuilder();
        result.AppendLine($"### {name} ({kind})");
        
        if (!string.IsNullOrEmpty(description))
        {
            result.AppendLine($"*{description}*\n");
        }

        switch (kind)
        {
            case "OBJECT":
            case "INPUT_OBJECT":
                if (type.TryGetProperty("fields", out var fields))
                {
                    result.AppendLine("**Fields:**");
                    foreach (var field in fields.EnumerateArray())
                    {
                        var fieldName = field.GetProperty("name").GetString();
                        var fieldType = GetTypeName(field.GetProperty("type"));
                        var fieldDesc = field.TryGetProperty("description", out var fd) ? fd.GetString() : "";
                        
                        result.AppendLine($"- `{fieldName}`: {fieldType}" + 
                            (!string.IsNullOrEmpty(fieldDesc) ? $" - {fieldDesc}" : ""));
                    }
                }
                break;

            case "ENUM":
                if (type.TryGetProperty("enumValues", out var enumValues))
                {
                    result.AppendLine("**Values:**");
                    foreach (var value in enumValues.EnumerateArray())
                    {
                        var valueName = value.GetProperty("name").GetString();
                        var valueDesc = value.TryGetProperty("description", out var vd) ? vd.GetString() : "";
                        
                        result.AppendLine($"- `{valueName}`" + 
                            (!string.IsNullOrEmpty(valueDesc) ? $" - {valueDesc}" : ""));
                    }
                }
                break;
        }

        return result.ToString();
    }

    private static string GetTypeName(JsonElement typeElement)
    {
        var kind = typeElement.GetProperty("kind").GetString();
        
        switch (kind)
        {
            case "NON_NULL":
                return GetTypeName(typeElement.GetProperty("ofType")) + "!";
            case "LIST":
                return "[" + GetTypeName(typeElement.GetProperty("ofType")) + "]";
            default:
                return typeElement.TryGetProperty("name", out var name) ? name.GetString() ?? "Unknown" : "Unknown";
        }
    }

    private static HashSet<string> GetTypeNames(JsonElement schema)
    {
        var typeNames = new HashSet<string>();
        
        if (schema.TryGetProperty("types", out var types))
        {
            foreach (var type in types.EnumerateArray())
            {
                if (type.TryGetProperty("name", out var name))
                {
                    var typeName = name.GetString();
                    if (!string.IsNullOrEmpty(typeName) && !typeName.StartsWith("__"))
                    {
                        typeNames.Add(typeName);
                    }
                }
            }
        }
        
        return typeNames;
    }
}
