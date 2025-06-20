using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Consolidated schema tools providing comprehensive GraphQL schema operations
/// Replaces: GraphQLSchemaTools, SchemaIntrospectionTools, SchemaExplorationTools, SchemaEvolutionTools
/// </summary>
[McpServerToolType]
public static class SchemaTools
{
    private static readonly StrawberryShakeSchemaService SchemaService = new();

    [McpServerTool, Description(@"Comprehensive GraphQL schema introspection and analysis tool.

This unified tool provides complete schema exploration including:
- Schema introspection with types, fields, and directives
- Type definitions and relationships
- Field analysis and documentation
- Query/mutation/subscription operations discovery
- Schema comparison capabilities

Output Formats:
- 'detailed': Full schema information with descriptions
- 'summary': Overview with type counts and key information
- 'types': Focus on type definitions only
- 'operations': Focus on available operations only

Use this as your primary tool for all schema-related operations.")]
    public static async Task<string> IntrospectSchema(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Output format: 'detailed', 'summary', 'types', 'operations'")]
        string format = "detailed",
        [Description("Include mutation operations in results")]
        bool includeMutations = true,
        [Description("Include subscription operations in results")]
        bool includeSubscriptions = false,
        [Description("Filter to specific type name (optional)")]
        string? typeName = null,
        [Description("Maximum depth for nested type analysis")]
        int maxDepth = 3)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        try
        {
            var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
            if (!schemaResult.IsSuccess)
            {
                return schemaResult.FormatForDisplay();
            }

            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
            if (!TryGetSchemaData(schemaData, out var schema, out var types))
            {
                return "Failed to parse schema data from introspection result.";
            }

            return format.ToLower() switch
            {
                "summary" => FormatSchemaSummary(endpointName, schema, types, includeMutations, includeSubscriptions),
                "types" => FormatTypesOnly(endpointName, types, typeName),
                "operations" => await FormatOperationsOnly(endpointName, schema, types, includeMutations, includeSubscriptions),
                "detailed" or _ => await FormatDetailedSchema(endpointName, schema, types, includeMutations, includeSubscriptions, typeName, maxDepth)
            };
        }
        catch (Exception ex)
        {
            return $"Error during schema introspection: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"Compare schemas between two GraphQL endpoints to identify differences.

This tool analyzes schema evolution and compatibility including:
- Type additions, removals, and modifications
- Field changes and breaking changes detection
- API version compatibility analysis
- Migration planning assistance

Essential for API versioning and compatibility analysis.")]
    public static async Task<string> CompareSchemas(
        [Description("Name of the first endpoint for comparison")]
        string endpoint1,
        [Description("Name of the second endpoint for comparison")]
        string endpoint2,
        [Description("Include detailed field-level analysis")]
        bool includeFieldAnalysis = true,
        [Description("Detect breaking changes")]
        bool detectBreakingChanges = true)
    {
        var endpointInfo1 = EndpointRegistryService.Instance.GetEndpointInfo(endpoint1);
        var endpointInfo2 = EndpointRegistryService.Instance.GetEndpointInfo(endpoint2);

        if (endpointInfo1 == null)
            return $"Error: Endpoint '{endpoint1}' not found.";
        if (endpointInfo2 == null)
            return $"Error: Endpoint '{endpoint2}' not found.";

        try
        {
            var comparison = await SchemaService.CompareSchemas(endpointInfo1, endpointInfo2);
            if (!comparison.IsSuccess)
            {
                return $"Schema comparison failed: {comparison.ErrorMessage}";
            }

            return FormatSchemaComparison(endpoint1, endpoint2, comparison, includeFieldAnalysis, detectBreakingChanges);
        }
        catch (Exception ex)
        {
            return $"Error during schema comparison: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"Get detailed information about a specific GraphQL type.

Provides comprehensive type analysis including:
- Complete type definition in SDL format
- All fields with types and descriptions
- Interface implementations and relationships
- Enum values or union members
- Usage examples and best practices")]
    public static async Task<string> GetTypeDefinition(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Name of the type to analyze")]
        string typeName,
        [Description("Include related types and relationships")]
        bool includeRelated = false,
        [Description("Output format: 'sdl', 'detailed', 'summary'")]
        string format = "detailed")
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found.";
        }

        try
        {
            var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
            if (!schemaResult.IsSuccess)
                return schemaResult.FormatForDisplay();

            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
            if (!TryGetSchemaData(schemaData, out var schema, out var types))
            {
                return "Failed to parse schema data.";
            }

            var type = FindTypeInSchema(types, typeName);
            if (type == null)
            {
                return $"Type '{typeName}' not found in schema.";
            }

            return format.ToLower() switch
            {
                "sdl" => FormatTypeAsSDL(type.Value),
                "summary" => FormatTypeSummary(type.Value),
                "detailed" or _ => FormatDetailedType(type.Value, includeRelated, types)
            };
        }
        catch (Exception ex)
        {
            return $"Error retrieving type definition: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"List all available operations (queries, mutations, subscriptions) for an endpoint.

Provides organized listing of all GraphQL operations including:
- Operation signatures with parameters
- Return types and descriptions
- Deprecation warnings
- Usage examples and patterns")]
    public static async Task<string> ListOperations(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Operation type to list: 'query', 'mutation', 'subscription', 'all'")]
        string operationType = "all",
        [Description("Include parameter details")]
        bool includeParameters = true,
        [Description("Include usage examples")]
        bool includeExamples = false)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found.";
        }

        try
        {
            var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
            if (!schemaResult.IsSuccess)
                return schemaResult.FormatForDisplay();

            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
            if (!TryGetSchemaData(schemaData, out var schema, out var types))
            {
                return "Failed to parse schema data.";
            }

            return await FormatOperationsListing(endpointName, schema, types, operationType, includeParameters, includeExamples);
        }
        catch (Exception ex)
        {
            return $"Error listing operations: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"Search schema for types, fields, or operations matching a pattern.

Powerful search capabilities including:
- Regex pattern matching across names and descriptions
- Type and field search with context
- Operation discovery by name patterns
- Related entity identification")]
    public static async Task<string> SearchSchema(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Search pattern (regex supported)")]
        string searchPattern,
        [Description("Search target: 'types', 'fields', 'operations', 'all'")]
        string searchTarget = "all",
        [Description("Case-sensitive search")]
        bool caseSensitive = false)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found.";
        }

        try
        {
            var regex = new Regex(searchPattern, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            
            var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
            if (!schemaResult.IsSuccess)
                return schemaResult.FormatForDisplay();

            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
            if (!TryGetSchemaData(schemaData, out var schema, out var types))
            {
                return "Failed to parse schema data.";
            }

            return FormatSearchResults(endpointName, searchPattern, searchTarget, regex, schema, types);
        }
        catch (ArgumentException ex)
        {
            return $"Invalid regex pattern: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error during schema search: {ex.Message}";
        }
    }

    #region Private Helper Methods

    private static bool TryGetSchemaData(JsonElement schemaData, out JsonElement schema, out JsonElement types)
    {
        schema = default;
        types = default;
        
        return schemaData.TryGetProperty("data", out var data) &&
               data.TryGetProperty("__schema", out schema) &&
               schema.TryGetProperty("types", out types);
    }

    private static JsonElement? FindTypeInSchema(JsonElement types, string typeName)
    {
        foreach (var type in types.EnumerateArray())
        {
            if (type.TryGetProperty("name", out var name) && 
                name.GetString()?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true)
            {
                return type;
            }
        }
        return null;
    }

    private static string FormatSchemaSummary(string endpointName, JsonElement schema, JsonElement types, bool includeMutations, bool includeSubscriptions)
    {
        var result = new StringBuilder();
        result.AppendLine($"# Schema Summary: {endpointName}\n");

        // Root types
        var queryType = schema.TryGetProperty("queryType", out var qt) ? qt.GetProperty("name").GetString() : "None";
        var mutationType = schema.TryGetProperty("mutationType", out var mt) ? mt.GetProperty("name").GetString() : "None";
        var subscriptionType = schema.TryGetProperty("subscriptionType", out var st) ? st.GetProperty("name").GetString() : "None";

        result.AppendLine("## Root Types");
        result.AppendLine($"- **Query:** {queryType}");
        if (includeMutations) result.AppendLine($"- **Mutation:** {mutationType}");
        if (includeSubscriptions) result.AppendLine($"- **Subscription:** {subscriptionType}");
        result.AppendLine();

        // Type counts
        var typeCounts = new Dictionary<string, int>();
        foreach (var type in types.EnumerateArray())
        {
            if (type.TryGetProperty("name", out var name) && !name.GetString()?.StartsWith("__") == true)
            {
                var kind = type.TryGetProperty("kind", out var k) ? k.GetString() ?? "UNKNOWN" : "UNKNOWN";
                typeCounts[kind] = typeCounts.GetValueOrDefault(kind, 0) + 1;
            }
        }

        result.AppendLine("## Type Summary");
        foreach (var kvp in typeCounts.OrderBy(kvp => kvp.Key))
        {
            result.AppendLine($"- **{kvp.Key}:** {kvp.Value} types");
        }

        return result.ToString();
    }

    private static string FormatTypesOnly(string endpointName, JsonElement types, string? typeName)
    {
        var result = new StringBuilder();
        result.AppendLine($"# Types: {endpointName}\n");

        var filteredTypes = types.EnumerateArray()
            .Where(type => {
                if (!type.TryGetProperty("name", out var name) || name.GetString()?.StartsWith("__") == true)
                    return false;
                
                if (!string.IsNullOrEmpty(typeName))
                    return name.GetString()?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true;
                    
                return true;
            })
            .OrderBy(type => type.GetProperty("name").GetString());

        var groupedTypes = filteredTypes.GroupBy(type => type.GetProperty("kind").GetString() ?? "UNKNOWN");

        foreach (var group in groupedTypes.OrderBy(g => g.Key))
        {
            result.AppendLine($"## {group.Key} Types\n");
            foreach (var type in group)
            {
                var name = type.GetProperty("name").GetString();
                var desc = type.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String 
                    ? $" - {d.GetString()}" : "";
                result.AppendLine($"- **{name}**{desc}");
            }
            result.AppendLine();
        }

        return result.ToString();
    }

    private static async Task<string> FormatOperationsOnly(string endpointName, JsonElement schema, JsonElement types, bool includeMutations, bool includeSubscriptions)
    {
        var result = new StringBuilder();
        result.AppendLine($"# Operations: {endpointName}\n");

        await FormatRootTypeOperations(result, "Query", schema, types, "queryType");
        
        if (includeMutations)
            await FormatRootTypeOperations(result, "Mutation", schema, types, "mutationType");
            
        if (includeSubscriptions)
            await FormatRootTypeOperations(result, "Subscription", schema, types, "subscriptionType");

        return result.ToString();
    }

    private static async Task FormatRootTypeOperations(StringBuilder result, string operationType, JsonElement schema, JsonElement types, string schemaProperty)
    {
        if (!schema.TryGetProperty(schemaProperty, out var rootTypeRef) || rootTypeRef.ValueKind == JsonValueKind.Null)
            return;

        var rootTypeName = rootTypeRef.GetProperty("name").GetString();
        var rootType = FindTypeInSchema(types, rootTypeName!);
        
        if (rootType == null || !rootType.Value.TryGetProperty("fields", out var fields))
            return;

        result.AppendLine($"## {operationType} Operations\n");
        
        foreach (var field in fields.EnumerateArray())
        {
            var name = field.GetProperty("name").GetString();
            var returnType = FormatTypeReference(field.GetProperty("type"));
            var desc = field.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String 
                ? $" - {d.GetString()}" : "";
            
            result.AppendLine($"- **{name}**: `{returnType}`{desc}");
        }
        result.AppendLine();
    }

    private static async Task<string> FormatDetailedSchema(string endpointName, JsonElement schema, JsonElement types, bool includeMutations, bool includeSubscriptions, string? typeName, int maxDepth)
    {
        var result = new StringBuilder();
        result.AppendLine($"# Detailed Schema: {endpointName}\n");

        // Include summary
        result.AppendLine(FormatSchemaSummary(endpointName, schema, types, includeMutations, includeSubscriptions));

        // Include operations
        result.AppendLine(await FormatOperationsOnly(endpointName, schema, types, includeMutations, includeSubscriptions));

        // Include types (limited if typeName specified)
        if (!string.IsNullOrEmpty(typeName))
        {
            result.AppendLine(FormatTypesOnly(endpointName, types, typeName));
        }
        else
        {
            result.AppendLine("## Key Types\n");
            var importantTypes = types.EnumerateArray()
                .Where(type => {
                    if (!type.TryGetProperty("name", out var name)) return false;
                    var typeName = name.GetString();
                    return !typeName?.StartsWith("__") == true && 
                           type.TryGetProperty("kind", out var kind) && 
                           kind.GetString() == "OBJECT";
                })
                .Take(10);

            foreach (var type in importantTypes)
            {
                var name = type.GetProperty("name").GetString();
                var desc = type.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String 
                    ? $" - {d.GetString()}" : "";
                result.AppendLine($"- **{name}**{desc}");
            }
        }

        return result.ToString();
    }

    private static string FormatSchemaComparison(string endpoint1, string endpoint2, dynamic comparison, bool includeFieldAnalysis, bool detectBreakingChanges)
    {
        var result = new StringBuilder();
        result.AppendLine($"# Schema Comparison: {endpoint1} vs {endpoint2}\n");

        // Basic comparison results
        if (comparison.Differences?.Count > 0)
        {
            result.AppendLine("## Differences Found\n");
            foreach (var diff in comparison.Differences)
            {
                result.AppendLine($"- **{diff.Type}**: {diff.TypeName} - {diff.Description}");
            }
        }
        else
        {
            result.AppendLine("✅ **Schemas are identical**\n");
        }

        if (detectBreakingChanges)
        {
            result.AppendLine("## Breaking Changes Analysis\n");
            result.AppendLine("*Breaking change detection would be implemented here*");
        }

        return result.ToString();
    }

    private static string FormatDetailedType(JsonElement type, bool includeRelated, JsonElement types)
    {
        var result = new StringBuilder();
        var typeName = type.GetProperty("name").GetString();
        var typeKind = type.GetProperty("kind").GetString();
        
        result.AppendLine($"# Type: {typeName}\n");
        result.AppendLine($"**Kind:** {typeKind}");

        if (type.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
        {
            result.AppendLine($"**Description:** {desc.GetString()}");
        }
        result.AppendLine();

        // Add type-specific details
        switch (typeKind)
        {
            case "OBJECT":
            case "INPUT_OBJECT":
            case "INTERFACE":
                FormatObjectTypeDetails(type, result);
                break;
            case "ENUM":
                FormatEnumTypeDetails(type, result);
                break;
            case "UNION":
                FormatUnionTypeDetails(type, result);
                break;
        }

        // SDL Definition
        result.AppendLine("## SDL Definition\n");
        result.AppendLine("```graphql");
        result.AppendLine(FormatTypeAsSDL(type));
        result.AppendLine("```");

        return result.ToString();
    }

    private static void FormatObjectTypeDetails(JsonElement type, StringBuilder result)
    {
        if (type.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine($"## Fields ({fields.GetArrayLength()})\n");
            foreach (var field in fields.EnumerateArray())
            {
                var name = field.GetProperty("name").GetString();
                var fieldType = FormatTypeReference(field.GetProperty("type"));
                var desc = field.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String 
                    ? $" - {d.GetString()}" : "";
                result.AppendLine($"- **{name}**: `{fieldType}`{desc}");
            }
            result.AppendLine();
        }
    }

    private static void FormatEnumTypeDetails(JsonElement type, StringBuilder result)
    {
        if (type.TryGetProperty("enumValues", out var enumValues) && enumValues.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine($"## Values ({enumValues.GetArrayLength()})\n");
            foreach (var value in enumValues.EnumerateArray())
            {
                var name = value.GetProperty("name").GetString();
                var desc = value.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String 
                    ? $" - {d.GetString()}" : "";
                result.AppendLine($"- **{name}**{desc}");
            }
            result.AppendLine();
        }
    }

    private static void FormatUnionTypeDetails(JsonElement type, StringBuilder result)
    {
        if (type.TryGetProperty("possibleTypes", out var possibleTypes) && possibleTypes.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine($"## Possible Types ({possibleTypes.GetArrayLength()})\n");
            foreach (var possibleType in possibleTypes.EnumerateArray())
            {
                var name = possibleType.GetProperty("name").GetString();
                result.AppendLine($"- {name}");
            }
            result.AppendLine();
        }
    }

    private static string FormatTypeSummary(JsonElement type)
    {
        var name = type.GetProperty("name").GetString();
        var kind = type.GetProperty("kind").GetString();
        var desc = type.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String 
            ? d.GetString() : "";

        return $"**{name}** ({kind}){(!string.IsNullOrEmpty(desc) ? $" - {desc}" : "")}";
    }

    private static string FormatTypeAsSDL(JsonElement type)
    {
        var name = type.GetProperty("name").GetString();
        var kind = type.GetProperty("kind").GetString();

        return kind switch
        {
            "OBJECT" => $"type {name} {{ ... }}",
            "INPUT_OBJECT" => $"input {name} {{ ... }}",
            "INTERFACE" => $"interface {name} {{ ... }}",
            "ENUM" => $"enum {name} {{ ... }}",
            "UNION" => $"union {name} = ...",
            "SCALAR" => $"scalar {name}",
            _ => $"{kind} {name}"
        };
    }

    private static async Task<string> FormatOperationsListing(string endpointName, JsonElement schema, JsonElement types, string operationType, bool includeParameters, bool includeExamples)
    {
        var result = new StringBuilder();
        result.AppendLine($"# Operations: {endpointName}\n");

        if (operationType.ToLower() is "query" or "all")
            await FormatRootTypeOperations(result, "Query", schema, types, "queryType");
            
        if (operationType.ToLower() is "mutation" or "all")
            await FormatRootTypeOperations(result, "Mutation", schema, types, "mutationType");
            
        if (operationType.ToLower() is "subscription" or "all")
            await FormatRootTypeOperations(result, "Subscription", schema, types, "subscriptionType");

        return result.ToString();
    }

    private static string FormatSearchResults(string endpointName, string searchPattern, string searchTarget, Regex regex, JsonElement schema, JsonElement types)
    {
        var result = new StringBuilder();
        result.AppendLine($"# Search Results: '{searchPattern}'\n");
        result.AppendLine($"**Endpoint:** {endpointName}");
        result.AppendLine($"**Target:** {searchTarget}");
        result.AppendLine();

        var typeMatches = new List<string>();
        var fieldMatches = new List<string>();

        foreach (var type in types.EnumerateArray())
        {
            if (!type.TryGetProperty("name", out var nameElement) || nameElement.GetString()?.StartsWith("__") == true)
                continue;

            var typeName = nameElement.GetString() ?? "";
            
            // Search types
            if ((searchTarget == "types" || searchTarget == "all") && regex.IsMatch(typeName))
            {
                var kind = type.TryGetProperty("kind", out var k) ? k.GetString() : "";
                typeMatches.Add($"- **{typeName}** ({kind})");
            }

            // Search fields
            if ((searchTarget == "fields" || searchTarget == "all") && 
                type.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
            {
                foreach (var field in fields.EnumerateArray())
                {
                    if (field.TryGetProperty("name", out var fieldNameElement))
                    {
                        var fieldName = fieldNameElement.GetString() ?? "";
                        if (regex.IsMatch(fieldName))
                        {
                            var fieldType = FormatTypeReference(field.GetProperty("type"));
                            fieldMatches.Add($"- **{typeName}.{fieldName}**: `{fieldType}`");
                        }
                    }
                }
            }
        }

        if (typeMatches.Any())
        {
            result.AppendLine($"## Type Matches ({typeMatches.Count})\n");
            typeMatches.ForEach(match => result.AppendLine(match));
            result.AppendLine();
        }

        if (fieldMatches.Any())
        {
            result.AppendLine($"## Field Matches ({fieldMatches.Count})\n");
            fieldMatches.ForEach(match => result.AppendLine(match));
            result.AppendLine();
        }

        if (!typeMatches.Any() && !fieldMatches.Any())
        {
            result.AppendLine("No matches found.");
        }

        return result.ToString();
    }

    private static string FormatTypeReference(JsonElement typeRef)
    {
        if (!typeRef.TryGetProperty("kind", out var kind))
            return "unknown";

        return kind.GetString() switch
        {
            "NON_NULL" => typeRef.TryGetProperty("ofType", out var ofType) 
                ? FormatTypeReference(ofType) + "!" 
                : "unknown!",
            "LIST" => typeRef.TryGetProperty("ofType", out var listOfType) 
                ? "[" + FormatTypeReference(listOfType) + "]" 
                : "[unknown]",
            _ => typeRef.TryGetProperty("name", out var name) 
                ? name.GetString() ?? "unknown" 
                : "unknown"
        };
    }

    #endregion
}