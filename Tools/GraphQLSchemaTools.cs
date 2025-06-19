using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;
using HotChocolate.Language;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class GraphQlSchemaTools
{
    private static readonly StrawberryShakeSchemaService _schemaService = new();
    [McpServerTool, Description("Retrieve and format specific GraphQL schema information with filtering and type focus")]
    public static async Task<string> GetSchema(
        [Description("Name of the registered GraphQL endpoint")] string endpointName,
        [Description("Type name to focus on (optional)")]
        string? typeName = null,
        [Description("Include only query types")]
        bool queryOnly = false,
        [Description("Include only mutation types")]
        bool mutationOnly = false)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var schemaResult = await _schemaService.GetSchemaAsync(endpointInfo);
        if (!schemaResult.IsSuccess)
        {
            return $"Failed to retrieve schema: {schemaResult.ErrorMessage}";
        }

        var schema = schemaResult.Schema!;
        var result = new StringBuilder();
        result.AppendLine("# GraphQL Schema\n");

        // Get root types using StrawberryShake
        var rootTypes = _schemaService.GetRootTypes(schema);
        
        result.AppendLine("## Root Types");
        result.AppendLine($"- **Query:** {rootTypes.QueryType}");
        result.AppendLine($"- **Mutation:** {rootTypes.MutationType ?? "None"}");
        result.AppendLine($"- **Subscription:** {rootTypes.SubscriptionType ?? "None"}\n");

        // Get type definitions
        var typeDefinitions = _schemaService.GetTypeDefinitions(schema);
        
        // Filter types based on parameters
        var filteredTypes = typeDefinitions.Where(def =>
        {
            if (def is not INamedSyntaxNode namedDef) return false;
            
            var currentTypeName = namedDef.Name.Value;
            
            // Skip introspection types
            if (currentTypeName.StartsWith("__")) return false;
            
            // Filter by specific type name if provided
            if (!string.IsNullOrEmpty(typeName) && 
                !currentTypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                return false;
            
            // Filter by operation type
            if (queryOnly && currentTypeName != rootTypes.QueryType)
                return false;
                
            if (mutationOnly && currentTypeName != rootTypes.MutationType)
                return false;
            
            return true;
        }).ToList();

        result.AppendLine("## Types\n");
        foreach (var typeDef in filteredTypes)
        {
            result.AppendLine(_schemaService.FormatTypeDefinition(typeDef));
            result.AppendLine();
        }

        return result.ToString();
    }

    [McpServerTool, Description("Analyze differences between two GraphQL schemas with detailed change reporting")]
    public static async Task<string> CompareSchemas(
        [Description("Name of the first registered GraphQL endpoint")]
        string endpointName1,
        [Description("Name of the second registered GraphQL endpoint")]
        string endpointName2)
    {
        var endpointInfo1 = EndpointRegistryService.Instance.GetEndpointInfo(endpointName1);
        if (endpointInfo1 == null)
        {
            return $"Error: Endpoint '{endpointName1}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var endpointInfo2 = EndpointRegistryService.Instance.GetEndpointInfo(endpointName2);
        if (endpointInfo2 == null)
        {
            return $"Error: Endpoint '{endpointName2}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var comparison = await _schemaService.CompareSchemas(endpointInfo1, endpointInfo2);
        if (!comparison.IsSuccess)
        {
            return $"Schema comparison failed: {comparison.ErrorMessage}";
        }

        var result = new StringBuilder();
        result.AppendLine("# Schema Comparison Report\n");
        result.AppendLine($"**Schema 1:** {endpointName1} ({endpointInfo1.Url})");
        result.AppendLine($"**Schema 2:** {endpointName2} ({endpointInfo2.Url})\n");

        var addedTypes = comparison.Differences.Where(d => d.Type == DifferenceType.TypeAdded).ToList();
        var removedTypes = comparison.Differences.Where(d => d.Type == DifferenceType.TypeRemoved).ToList();
        var totalDifferences = comparison.Differences.Count;

        result.AppendLine("## Type Changes\n");

        if (addedTypes.Any())
        {
            result.AppendLine("### Added Types");
            foreach (var diff in addedTypes)
            {
                result.AppendLine($"+ {diff.TypeName} - {diff.Description}");
            }
            result.AppendLine();
        }

        if (removedTypes.Any())
        {
            result.AppendLine("### Removed Types");
            foreach (var diff in removedTypes)
            {
                result.AppendLine($"- {diff.TypeName} - {diff.Description}");
            }
            result.AppendLine();
        }

        result.AppendLine("### Summary");
        result.AppendLine($"- **Added types:** {addedTypes.Count}");
        result.AppendLine($"- **Removed types:** {removedTypes.Count}");
        result.AppendLine($"- **Total differences:** {totalDifferences}");

        if (totalDifferences == 0)
        {
            result.AppendLine("\n🎉 **Schemas are identical!**");
        }

        return result.ToString();
    }

    [McpServerTool, Description("Execute the same GraphQL query on two endpoints and compare their responses")]
    public static async Task<string> CompareRequestResponses(
        [Description("GraphQL query to execute on both services")]
        string query,
        [Description("Name of the first registered GraphQL endpoint")]
        string endpointName1,
        [Description("Name of the second registered GraphQL endpoint")]
        string endpointName2,
        [Description("GraphQL variables as JSON object (optional)")]
        string? variables = null,
        [Description("Include response timing comparison")]
        bool includeTiming = true,
        [Description("Show detailed diff of response data")]
        bool detailedDiff = true)
    {
        var endpointInfo1 = EndpointRegistryService.Instance.GetEndpointInfo(endpointName1);
        if (endpointInfo1 == null)
        {
            return $"Error: Endpoint '{endpointName1}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var endpointInfo2 = EndpointRegistryService.Instance.GetEndpointInfo(endpointName2);
        if (endpointInfo2 == null)
        {
            return $"Error: Endpoint '{endpointName2}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var result = new StringBuilder();
        result.AppendLine("# GraphQL Request Comparison Report\n");
        result.AppendLine($"**Query:**\n```graphql\n{query}\n```\n");
        result.AppendLine($"**Service 1:** {endpointName1} ({endpointInfo1.Url})");
        result.AppendLine($"**Service 2:** {endpointName2} ({endpointInfo2.Url})\n");

        if (!string.IsNullOrEmpty(variables))
        {
            result.AppendLine($"**Variables:**\n```json\n{variables}\n```\n");
        }

        // Execute request on both services with timing
        var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
        var response1 = await ExecuteGraphQlRequest(endpointInfo1, query, variables);
        stopwatch1.Stop();

        var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
        var response2 = await ExecuteGraphQlRequest(endpointInfo2, query, variables);
        stopwatch2.Stop();

        // Parse responses
        JsonElement? data1 = null, data2 = null;
        JsonElement? errors1 = null, errors2 = null;
        bool parseSuccess1 = false, parseSuccess2 = false;

        try
        {
            var json1 = JsonSerializer.Deserialize<JsonElement>(response1);
            parseSuccess1 = true;
            json1.TryGetProperty("data", out var d1);
            data1 = d1;
            json1.TryGetProperty("errors", out var e1);
            errors1 = e1;
        }
        catch (Exception ex)
        {
            result.AppendLine($"**⚠️ Service 1 Response Parse Error:** {ex.Message}\n");
        }

        try
        {
            var json2 = JsonSerializer.Deserialize<JsonElement>(response2);
            parseSuccess2 = true;
            json2.TryGetProperty("data", out var d2);
            data2 = d2;
            json2.TryGetProperty("errors", out var e2);
            errors2 = e2;
        }
        catch (Exception ex)
        {
            result.AppendLine($"**⚠️ Service 2 Response Parse Error:** {ex.Message}\n");
        }

        // Timing comparison
        if (includeTiming)
        {
            result.AppendLine("## Performance Comparison\n");
            result.AppendLine($"| Service | Response Time | Status |");
            result.AppendLine($"|---------|---------------|--------|");
            result.AppendLine($"| Service 1 | {stopwatch1.ElapsedMilliseconds}ms | {(parseSuccess1 ? "✅ Success" : "❌ Failed")} |");
            result.AppendLine($"| Service 2 | {stopwatch2.ElapsedMilliseconds}ms | {(parseSuccess2 ? "✅ Success" : "❌ Failed")} |");

            var timeDiff = Math.Abs(stopwatch1.ElapsedMilliseconds - stopwatch2.ElapsedMilliseconds);
            var fasterService = stopwatch1.ElapsedMilliseconds < stopwatch2.ElapsedMilliseconds ? "Service 1" : "Service 2";
            result.AppendLine($"\n**Performance Summary:** {fasterService} is {timeDiff}ms faster\n");
        }

        // Error comparison
        var hasErrors1 = errors1.HasValue && errors1.Value.ValueKind == JsonValueKind.Array && errors1.Value.GetArrayLength() > 0;
        var hasErrors2 = errors2.HasValue && errors2.Value.ValueKind == JsonValueKind.Array && errors2.Value.GetArrayLength() > 0;

        if (hasErrors1 || hasErrors2)
        {
            result.AppendLine("## Error Comparison\n");

            if (hasErrors1 && errors1.HasValue)
            {
                result.AppendLine("### Service 1 Errors");
                foreach (var error in errors1.Value.EnumerateArray())
                {
                    var message = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                    result.AppendLine($"- {message}");
                }

                result.AppendLine();
            }

            if (hasErrors2 && errors2.HasValue)
            {
                result.AppendLine("### Service 2 Errors");
                foreach (var error in errors2.Value.EnumerateArray())
                {
                    var message = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                    result.AppendLine($"- {message}");
                }

                result.AppendLine();
            }

            // Error status summary
            if (hasErrors1 && hasErrors2)
            {
                result.AppendLine("**Status:** Both services returned errors\n");
            }
            else if (hasErrors1)
            {
                result.AppendLine("**Status:** Only Service 1 returned errors\n");
            }
            else if (hasErrors2)
            {
                result.AppendLine("**Status:** Only Service 2 returned errors\n");
            }
        }

        // Data comparison
        if (data1.HasValue && data2.HasValue && parseSuccess1 && parseSuccess2)
        {
            result.AppendLine("## Data Comparison\n");

            var data1Json = JsonSerializer.Serialize(data1.Value, new JsonSerializerOptions { WriteIndented = true });
            var data2Json = JsonSerializer.Serialize(data2.Value, new JsonSerializerOptions { WriteIndented = true });

            if (data1Json == data2Json)
            {
                result.AppendLine("✅ **Data Match:** Both services returned identical data\n");
            }
            else
            {
                result.AppendLine("❌ **Data Mismatch:** Services returned different data\n");

                if (detailedDiff)
                {
                    result.AppendLine("### Service 1 Response");
                    result.AppendLine($"```json\n{data1Json}\n```\n");

                    result.AppendLine("### Service 2 Response");
                    result.AppendLine($"```json\n{data2Json}\n```\n");

                    // Simple field-level comparison
                    result.AppendLine("### Key Differences");
                    var differences = FindJsonDifferences(data1.Value, data2.Value);
                    if (differences.Any())
                    {
                        foreach (var diff in differences)
                        {
                            result.AppendLine($"- {diff}");
                        }
                    }
                    else
                    {
                        result.AppendLine("- Structures are similar but values may differ");
                    }

                    result.AppendLine();
                }
            }
        }
        else if (!parseSuccess1 || !parseSuccess2)
        {
            result.AppendLine("## Raw Response Comparison\n");

            if (!parseSuccess1)
            {
                result.AppendLine("### Service 1 Raw Response");
                result.AppendLine($"```\n{response1}\n```\n");
            }

            if (!parseSuccess2)
            {
                result.AppendLine("### Service 2 Raw Response");
                result.AppendLine($"```\n{response2}\n```\n");
            }
        }

        // Summary
        result.AppendLine("## Summary\n");
        if (parseSuccess1 && parseSuccess2 && !hasErrors1 && !hasErrors2)
        {
            if (data1.HasValue && data2.HasValue)
            {
                var data1Json = JsonSerializer.Serialize(data1.Value);
                var data2Json = JsonSerializer.Serialize(data2.Value);

                if (data1Json == data2Json)
                {
                    result.AppendLine("🎉 **Perfect Match:** Both services are fully compatible");
                }
                else
                {
                    result.AppendLine("⚠️ **Data Differences:** Services are functional but return different data");
                }
            }
        }
        else if (hasErrors1 || hasErrors2)
        {
            result.AppendLine("❌ **Errors Present:** One or both services have issues");
        }
        else
        {
            result.AppendLine("❓ **Inconclusive:** Unable to properly compare responses");
        }

        return result.ToString();
    }

    private static async Task<string> ExecuteGraphQlRequest(GraphQlEndpointInfo endpointInfo, string query, string? variables)
    {
        var requestBody = new
        {
            query = query,
            variables = !string.IsNullOrEmpty(variables) ? JsonSerializer.Deserialize<object>(variables) : null
        };

        var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, requestBody);

        if (!result.IsSuccess)
        {
            // Return formatted error for debugging - this will show connection issues clearly
            return result.FormatForDisplay();
        }

        return result.Content ?? "";
    }

    private static List<string> FindJsonDifferences(JsonElement json1, JsonElement json2, string path = "")
    {
        var differences = new List<string>();

        if (json1.ValueKind != json2.ValueKind)
        {
            differences.Add($"Type mismatch at '{path}': {json1.ValueKind} vs {json2.ValueKind}");
            return differences;
        }

        switch (json1.ValueKind)
        {
            case JsonValueKind.Object:
                var props1 = json1.EnumerateObject()
                    .Select(p => p.Name)
                    .ToHashSet();
                var props2 = json2.EnumerateObject()
                    .Select(p => p.Name)
                    .ToHashSet();

                foreach (var prop in props1.Union(props2))
                {
                    var newPath = string.IsNullOrEmpty(path) ? prop : $"{path}.{prop}";

                    if (!props1.Contains(prop))
                    {
                        differences.Add($"Property missing in Service 1: '{newPath}'");
                    }
                    else if (!props2.Contains(prop))
                    {
                        differences.Add($"Property missing in Service 2: '{newPath}'");
                    }
                    else
                    {
                        differences.AddRange(FindJsonDifferences(
                            json1.GetProperty(prop),
                            json2.GetProperty(prop),
                            newPath));
                    }
                }

                break;

            case JsonValueKind.Array:
                var array1 = json1.EnumerateArray()
                    .ToArray();
                var array2 = json2.EnumerateArray()
                    .ToArray();

                if (array1.Length != array2.Length)
                {
                    differences.Add($"Array length mismatch at '{path}': {array1.Length} vs {array2.Length}");
                }

                var minLength = Math.Min(array1.Length, array2.Length);
                for (var i = 0; i < minLength; i++)
                {
                    differences.AddRange(FindJsonDifferences(
                        array1[i],
                        array2[i],
                        $"{path}[{i}]"));
                }

                break;

            default:
                var value1 = json1.ToString();
                var value2 = json2.ToString();
                if (value1 != value2)
                {
                    differences.Add($"Value mismatch at '{path}': '{value1}' vs '{value2}'");
                }

                break;
        }

        return differences;
    }

    private static string FormatTypeDefinition(JsonElement type)
    {
        var name = type.GetProperty("name")
            .GetString();
        var kind = type.GetProperty("kind")
            .GetString();
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
                        var fieldName = field.GetProperty("name")
                            .GetString();
                        var fieldType = GraphQlTypeHelpers.GetTypeName(field.GetProperty("type"));
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
                        var valueName = value.GetProperty("name")
                            .GetString();
                        var valueDesc = value.TryGetProperty("description", out var vd) ? vd.GetString() : "";

                        result.AppendLine($"- `{valueName}`" +
                                          (!string.IsNullOrEmpty(valueDesc) ? $" - {valueDesc}" : ""));
                    }
                }

                break;
        }

        return result.ToString();
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