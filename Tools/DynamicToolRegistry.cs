using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace Tools;

/// <summary>
/// Dynamic tool registry that generates MCP tools from GraphQL schema operations
/// </summary>
[McpServerToolType]
public static class DynamicToolRegistry
{
    private static readonly Dictionary<string, DynamicToolInfo> _dynamicTools = new();
    private static readonly Dictionary<string, GraphQLEndpointInfo> _endpoints = new();

    [McpServerTool, Description("Register a GraphQL endpoint for automatic tool generation")]
    public static async Task<string> RegisterEndpoint(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Unique name for this endpoint")] string endpointName,
        [Description("HTTP headers as JSON object (optional)")] string? headers = null,
        [Description("Allow mutations to be registered as tools")] bool allowMutations = false,
        [Description("Tool prefix for generated tools")] string toolPrefix = "")
    {
        try
        {
            // Parse headers
            var headerDict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(headers))
            {
                try
                {
                    headerDict = JsonSerializer.Deserialize<Dictionary<string, string>>(headers) ?? new();
                }
                catch (JsonException ex)
                {
                    return $"Error parsing headers JSON: {ex.Message}";
                }
            }

            // Store endpoint info
            var endpointInfo = new GraphQLEndpointInfo
            {
                Name = endpointName,
                Url = endpoint,
                Headers = headerDict,
                AllowMutations = allowMutations,
                ToolPrefix = toolPrefix
            };

            _endpoints[endpointName] = endpointInfo;

            // Generate tools from schema
            var result = await GenerateToolsFromSchema(endpointInfo);
            return result;
        }
        catch (Exception ex)
        {
            return $"Error registering endpoint: {ex.Message}";
        }
    }

    [McpServerTool, Description("List all registered dynamic tools")]
    public static string ListDynamicTools()
    {
        if (_dynamicTools.Count == 0)
        {
            return "No dynamic tools are currently registered. Use RegisterEndpoint to generate tools from a GraphQL schema.";
        }

        var result = new StringBuilder();
        result.AppendLine("# Registered Dynamic Tools\n");

        var groupedByEndpoint = _dynamicTools.Values.GroupBy(t => t.EndpointName);

        foreach (var group in groupedByEndpoint)
        {
            result.AppendLine($"## Endpoint: {group.Key}");
            result.AppendLine($"**URL:** {_endpoints[group.Key].Url}");
            result.AppendLine($"**Operations:** {group.Count()}");
            result.AppendLine();

            var queries = group.Where(t => t.OperationType == "Query").ToList();
            var mutations = group.Where(t => t.OperationType == "Mutation").ToList();

            if (queries.Any())
            {
                result.AppendLine("### Queries");
                foreach (var query in queries)
                {
                    result.AppendLine($"- **{query.ToolName}**: {query.Description}");
                }
                result.AppendLine();
            }

            if (mutations.Any())
            {
                result.AppendLine("### Mutations");
                foreach (var mutation in mutations)
                {
                    result.AppendLine($"- **{mutation.ToolName}**: {mutation.Description}");
                }
                result.AppendLine();
            }
        }

        return result.ToString();
    }

    [McpServerTool, Description("Execute a dynamically generated GraphQL operation")]
    public static async Task<string> ExecuteDynamicOperation(
        [Description("Name of the dynamic tool to execute")] string toolName,
        [Description("Variables for the operation as JSON object")] string? variables = null)
    {
        try
        {
            if (!_dynamicTools.TryGetValue(toolName, out var toolInfo))
            {
                return $"Dynamic tool '{toolName}' not found. Use ListDynamicTools to see available tools.";
            }

            var endpointInfo = _endpoints[toolInfo.EndpointName];

            // Parse variables
            var variableDict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(variables))
            {
                try
                {
                    using var document = JsonDocument.Parse(variables);
                    variableDict = JsonElementToDictionary(document.RootElement);
                }
                catch (JsonException ex)
                {
                    return $"Error parsing variables JSON: {ex.Message}";
                }
            }        // Execute the operation
        using var httpClient = new HttpClient();
        
        // Configure headers using the centralized helper with dictionary overload
        HttpClientHelper.ConfigureHeaders(httpClient, endpointInfo.Headers);

        var request = new
        {
            query = toolInfo.Operation,
            variables = variableDict.Count > 0 ? variableDict : null,
            operationName = toolInfo.OperationName
        };

        var content = HttpClientHelper.CreateGraphQLContent(request);
            var response = await httpClient.PostAsync(endpointInfo.Url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"HTTP Error {response.StatusCode}: {responseContent}";
            }

            return FormatGraphQLResponse(responseContent);
        }
        catch (Exception ex)
        {
            return $"Error executing dynamic operation: {ex.Message}";
        }
    }

    [McpServerTool, Description("Refresh tools for a registered endpoint (re-introspect schema)")]
    public static async Task<string> RefreshEndpointTools(
        [Description("Name of the endpoint to refresh")] string endpointName)
    {
        try
        {
            if (!_endpoints.TryGetValue(endpointName, out var endpointInfo))
            {
                return $"Endpoint '{endpointName}' not found. Use RegisterEndpoint first.";
            }

            // Remove existing tools for this endpoint
            var keysToRemove = _dynamicTools.Where(kvp => kvp.Value.EndpointName == endpointName)
                                          .Select(kvp => kvp.Key)
                                          .ToList();
            
            foreach (var key in keysToRemove)
            {
                _dynamicTools.Remove(key);
            }

            // Re-generate tools
            var result = await GenerateToolsFromSchema(endpointInfo);
            return $"Refreshed tools for endpoint '{endpointName}'. {result}";
        }
        catch (Exception ex)
        {
            return $"Error refreshing endpoint tools: {ex.Message}";
        }
    }

    [McpServerTool, Description("Remove all dynamic tools for an endpoint")]
    public static string UnregisterEndpoint(
        [Description("Name of the endpoint to unregister")] string endpointName)
    {
        try
        {
            if (!_endpoints.TryGetValue(endpointName, out var endpointInfo))
            {
                return $"Endpoint '{endpointName}' not found.";
            }

            // Remove all tools for this endpoint
            var keysToRemove = _dynamicTools.Where(kvp => kvp.Value.EndpointName == endpointName)
                                          .Select(kvp => kvp.Key)
                                          .ToList();
            
            foreach (var key in keysToRemove)
            {
                _dynamicTools.Remove(key);
            }

            // Remove endpoint
            _endpoints.Remove(endpointName);

            return $"Unregistered endpoint '{endpointName}' and removed {keysToRemove.Count} dynamic tools.";
        }
        catch (Exception ex)
        {
            return $"Error unregistering endpoint: {ex.Message}";
        }
    }

    private static async Task<string> GenerateToolsFromSchema(GraphQLEndpointInfo endpointInfo)
    {
        try
        {
            // Introspect the schema
            var headersJson = endpointInfo.Headers.Count > 0 
                ? JsonSerializer.Serialize(endpointInfo.Headers)
                : null;

            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo.Url, headersJson);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            if (!schemaData.TryGetProperty("data", out var data) ||
                !data.TryGetProperty("__schema", out var schema))
            {
                return "Failed to parse schema introspection data";
            }

            var toolsGenerated = 0;

            // Find Query type
            if (schema.TryGetProperty("queryType", out var queryTypeRef) &&
                queryTypeRef.TryGetProperty("name", out var queryTypeName))
            {
                var queryType = FindTypeByName(schema, queryTypeName.GetString() ?? "Query");
                if (queryType.HasValue)
                {
                    var queryTools = GenerateToolsForType(queryType.Value, "Query", endpointInfo);
                    toolsGenerated += queryTools;
                }
            }

            // Find Mutation type (if allowed)
            if (endpointInfo.AllowMutations &&
                schema.TryGetProperty("mutationType", out var mutationTypeRef) &&
                mutationTypeRef.TryGetProperty("name", out var mutationTypeName))
            {
                var mutationType = FindTypeByName(schema, mutationTypeName.GetString() ?? "Mutation");
                if (mutationType.HasValue)
                {
                    var mutationTools = GenerateToolsForType(mutationType.Value, "Mutation", endpointInfo);
                    toolsGenerated += mutationTools;
                }
            }

            var result = new StringBuilder();
            result.AppendLine($"Generated {toolsGenerated} dynamic tools for endpoint '{endpointInfo.Name}'");
            
            if (!endpointInfo.AllowMutations)
            {
                result.AppendLine("Note: Mutations were not enabled for this endpoint");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error generating tools from schema: {ex.Message}";
        }
    }

    private static JsonElement? FindTypeByName(JsonElement schema, string typeName)
    {
        if (!schema.TryGetProperty("types", out var types) || types.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var type in types.EnumerateArray())
        {
            if (type.TryGetProperty("name", out var name) && name.GetString() == typeName)
            {
                return type;
            }
        }

        return null;
    }

    private static int GenerateToolsForType(JsonElement type, string operationType, GraphQLEndpointInfo endpointInfo)
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

            // Generate the GraphQL operation
            var operation = GenerateOperationString(field, operationType, fieldNameStr);
            var description = GetFieldDescription(field, operationType, fieldNameStr);
            var operationName = $"{operationType}_{fieldNameStr}";

            var toolInfo = new DynamicToolInfo
            {
                ToolName = toolName,
                EndpointName = endpointInfo.Name,
                OperationType = operationType,
                OperationName = operationName,
                Operation = operation,
                Description = description,
                Field = field
            };

            _dynamicTools[toolName] = toolInfo;
            toolsGenerated++;
        }

        return toolsGenerated;
    }

    private static string GenerateToolName(string prefix, string operationType, string fieldName)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(prefix))
            parts.Add(prefix);
        
        parts.Add(operationType.ToLower());
        parts.Add(ToCamelCase(fieldName));

        return string.Join("_", parts);
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }

    private static string GenerateOperationString(JsonElement field, string operationType, string fieldName)
    {
        var operation = new StringBuilder();
        
        operation.AppendLine($"{operationType.ToLower()} {operationType}_{fieldName}(");
        
        // Add parameters
        if (field.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Array)
        {
            var parameters = new List<string>();
            foreach (var arg in args.EnumerateArray())
            {
                if (arg.TryGetProperty("name", out var argName) &&
                    arg.TryGetProperty("type", out var argType))
                {
                    var paramName = argName.GetString() ?? "";
                    var paramType = GraphQLTypeHelpers.GetTypeName(argType);
                    parameters.Add($"${paramName}: {paramType}");
                }
            }
            
            if (parameters.Count > 0)
            {
                operation.AppendLine(string.Join(",\n  ", parameters));
            }
        }
        
        operation.AppendLine(") {");
        
        // Add field call
        operation.Append($"  {fieldName}");
        
        // Add arguments if any
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
        
        // Add basic field selection (could be enhanced)
        operation.AppendLine(" {");
        operation.AppendLine("    # Add your field selections here");
        operation.AppendLine("    # This is a template - customize the fields you need");
        operation.AppendLine("  }");
        operation.AppendLine("}");
        
        return operation.ToString();
    }

    private static string GetFieldDescription(JsonElement field, string operationType, string fieldName)
    {
        var description = new StringBuilder();
        
        if (field.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
        {
            description.Append(desc.GetString());
        }
        else
        {
            description.Append($"Execute {operationType.ToLower()} operation: {fieldName}");
        }

        // Add parameter info
        if (field.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Array)
        {
            var argCount = args.GetArrayLength();
            if (argCount > 0)
            {
                description.Append($" (requires {argCount} parameter{(argCount == 1 ? "" : "s")})");
            }
        }

        return description.ToString();
    }

    private static Dictionary<string, object> JsonElementToDictionary(JsonElement element)
    {
        var dictionary = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = JsonElementToObject(property.Value);
        }

        return dictionary;
    }

    private static object JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToArray(),
            JsonValueKind.Object => JsonElementToDictionary(element),
            _ => element.ToString()
        };
    }

    private static string FormatGraphQLResponse(string responseContent)
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

    private class GraphQLEndpointInfo
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public Dictionary<string, string> Headers { get; set; } = new();
        public bool AllowMutations { get; set; }
        public string ToolPrefix { get; set; } = "";
    }

    private class DynamicToolInfo
    {
        public string ToolName { get; set; } = "";
        public string EndpointName { get; set; } = "";
        public string OperationType { get; set; } = "";
        public string OperationName { get; set; } = "";
        public string Operation { get; set; } = "";
        public string Description { get; set; } = "";
        public JsonElement Field { get; set; }
    }
}
