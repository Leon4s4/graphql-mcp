using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class GraphQlSchemaTools
{
    [McpServerTool, Description("Get a focused view of the GraphQL schema with specific type information")]
    public static async Task<string> GetSchema(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Type name to focus on (optional)")] string? typeName = null,
        [Description("Include only query types")] bool queryOnly = false,
        [Description("Include only mutation types")] bool mutationOnly = false,
        [Description("HTTP headers as JSON object (optional)")] string? headers = null)
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

    [McpServerTool, Description("Compare two GraphQL schemas and show differences")]
    public static async Task<string> CompareSchemas(
        [Description("First GraphQL endpoint URL")] string endpoint1,
        [Description("Second GraphQL endpoint URL")] string endpoint2,
        [Description("HTTP headers for first endpoint as JSON (optional)")] string? headers1 = null,
        [Description("HTTP headers for second endpoint as JSON (optional)")] string? headers2 = null)
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

    [McpServerTool, Description("Compare the same GraphQL request across two different services")]
    public static async Task<string> CompareRequestResponses(
        [Description("GraphQL query to execute on both services")] string query,
        [Description("First GraphQL endpoint URL")] string endpoint1,
        [Description("Second GraphQL endpoint URL")] string endpoint2,
        [Description("GraphQL variables as JSON object (optional)")] string? variables = null,
        [Description("HTTP headers for first endpoint as JSON (optional)")] string? headers1 = null,
        [Description("HTTP headers for second endpoint as JSON (optional)")] string? headers2 = null,
        [Description("Include response timing comparison")] bool includeTiming = true,
        [Description("Show detailed diff of response data")] bool detailedDiff = true)
    {
            var result = new StringBuilder();
            result.AppendLine("# GraphQL Request Comparison Report\n");
            result.AppendLine($"**Query:**\n```graphql\n{query}\n```\n");
            result.AppendLine($"**Service 1:** {endpoint1}");
            result.AppendLine($"**Service 2:** {endpoint2}\n");

            if (!string.IsNullOrEmpty(variables))
            {
                result.AppendLine($"**Variables:**\n```json\n{variables}\n```\n");
            }

            // Execute request on both services with timing
            var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
            var response1 = await ExecuteGraphQlRequest(endpoint1, query, variables, headers1);
            stopwatch1.Stop();

            var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
            var response2 = await ExecuteGraphQlRequest(endpoint2, query, variables, headers2);
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
                
                if (hasErrors1)
                {
                    result.AppendLine("### Service 1 Errors");
                    foreach (var error in errors1.Value.EnumerateArray())
                    {
                        var message = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                        result.AppendLine($"- {message}");
                    }
                    result.AppendLine();
                }

                if (hasErrors2)
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

    private static async Task<string> ExecuteGraphQlRequest(string endpoint, string query, string? variables, string? headers)
    {
        var requestBody = new
        {
            query = query,
            variables = !string.IsNullOrEmpty(variables) ? JsonSerializer.Deserialize<object>(variables) : null
        };

        var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpoint, requestBody, headers);
        
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
                var props1 = json1.EnumerateObject().Select(p => p.Name).ToHashSet();
                var props2 = json2.EnumerateObject().Select(p => p.Name).ToHashSet();

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
                var array1 = json1.EnumerateArray().ToArray();
                var array2 = json2.EnumerateArray().ToArray();

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
