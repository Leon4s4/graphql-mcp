using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace Tools;

/// <summary>
/// Dynamic tool registry that generates MCP tools from GraphQL schema operations
/// Uses EndpointRegistryService singleton to persist data across MCP tool calls
/// </summary>
[McpServerToolType]
public static class DynamicToolRegistry
{
    private static EndpointRegistryService Registry => EndpointRegistryService.Instance;

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

            // Register endpoint using singleton service
            Registry.RegisterEndpoint(endpointName, endpointInfo);

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
        var dynamicTools = Registry.GetAllDynamicTools();
        
        if (dynamicTools.Count == 0)
        {
            return "No dynamic tools are currently registered. Use RegisterEndpoint to generate tools from a GraphQL schema.";
        }

        var result = new StringBuilder();
        result.AppendLine("# Registered Dynamic Tools\n");

        var groupedByEndpoint = dynamicTools.Values.GroupBy(t => t.EndpointName);

        foreach (var group in groupedByEndpoint)
        {
            var endpoint = Registry.GetEndpointInfo(group.Key);
            if (endpoint == null) continue;
            
            result.AppendLine($"## Endpoint: {group.Key}");
            result.AppendLine($"**URL:** {endpoint.Url}");
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
            var toolInfo = Registry.GetDynamicTool(toolName);
            if (toolInfo == null)
            {
                return $"Dynamic tool '{toolName}' not found. Use ListDynamicTools to see available tools.";
            }

            var endpointInfo = Registry.GetEndpointInfo(toolInfo.EndpointName);
            if (endpointInfo == null)
            {
                return $"Endpoint '{toolInfo.EndpointName}' not found for tool '{toolName}'.";
            }

            // Parse variables
            var variableDict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(variables))
            {
                try
                {
                    using var document = JsonDocument.Parse(variables);
                    variableDict = JsonHelpers.JsonElementToDictionary(document.RootElement);
                }
                catch (JsonException ex)
                {
                    return $"Error parsing variables JSON: {ex.Message}";
                }
            }
            
            // Execute the operation using centralized HTTP helper
            var request = new
            {
                query = toolInfo.Operation,
                variables = variableDict.Count > 0 ? variableDict : null,
                operationName = toolInfo.OperationName
            };

            var result = await HttpClientHelper.ExecuteGraphQLRequestAsync(
                endpointInfo.Url,
                request,
                endpointInfo.Headers);

            if (!result.IsSuccess)
            {
                // Format the error using HttpClientHelper's response formatting
                return result.FormatForDisplay();
            }

            return FormatGraphQLResponse(result.Content!);
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
        var endpointInfo = Registry.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Endpoint '{endpointName}' not found. Use RegisterEndpoint first.";
        }

        // Remove existing tools for this endpoint (but keep the endpoint)
        var toolsRemoved = Registry.RemoveToolsForEndpoint(endpointName);

        // Re-generate tools
        var result = await GenerateToolsFromSchema(endpointInfo);
        return $"Refreshed tools for endpoint '{endpointName}'. Removed {toolsRemoved} existing tools. {result}";
    }

    [McpServerTool, Description("Remove all dynamic tools for an endpoint")]
    public static string UnregisterEndpoint(
        [Description("Name of the endpoint to unregister")] string endpointName)
    {
        if (Registry.RemoveEndpoint(endpointName, out var toolsRemoved))
        {
            return $"Unregistered endpoint '{endpointName}' and removed {toolsRemoved} dynamic tools.";
        }
        else
        {
            return $"Endpoint '{endpointName}' not found.";
        }
    }

    [McpServerTool, Description("Remove multiple dynamic tools for multiple endpoints by name")]
    public static string UnregisterMultipleEndpoints(
        [Description("Comma-separated list of endpoint names to unregister")] string endpointNames)
    {
        if (string.IsNullOrWhiteSpace(endpointNames))
        {
            return "No endpoint names provided. Please specify a comma-separated list of endpoint names.";
        }

        var names = endpointNames.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(name => name.Trim())
                                .Where(name => !string.IsNullOrEmpty(name))
                                .ToList();

        if (names.Count == 0)
        {
            return "No valid endpoint names found after parsing the input.";
        }

        var result = new StringBuilder();
        result.AppendLine($"# Unregistering {names.Count} Endpoint(s)\n");

        var totalToolsRemoved = 0;
        var successCount = 0;
        var failedEndpoints = new List<string>();

        foreach (var endpointName in names)
        {
            if (Registry.RemoveEndpoint(endpointName, out var toolsRemoved))
            {
                result.AppendLine($"✅ **{endpointName}**: Removed {toolsRemoved} tools");
                totalToolsRemoved += toolsRemoved;
                successCount++;
            }
            else
            {
                result.AppendLine($"❌ **{endpointName}**: Endpoint not found");
                failedEndpoints.Add(endpointName);
            }
        }

        result.AppendLine();
        result.AppendLine($"## Summary");
        result.AppendLine($"- **Successfully unregistered:** {successCount} endpoint(s)");
        result.AppendLine($"- **Failed to unregister:** {failedEndpoints.Count} endpoint(s)");
        result.AppendLine($"- **Total tools removed:** {totalToolsRemoved}");

        if (failedEndpoints.Count > 0)
        {
            result.AppendLine($"- **Failed endpoints:** {string.Join(", ", failedEndpoints)}");
        }

        return result.ToString();
    }

    [McpServerTool, Description("List all registered GraphQL endpoints")]
    public static string ListRegisteredEndpoints()
    {
        var endpoints = Registry.GetAllEndpoints();
        
        if (endpoints.Count == 0)
        {
            return "No GraphQL endpoints are currently registered. Use RegisterEndpoint to add endpoints.";
        }

        var result = new StringBuilder();
        result.AppendLine("# Registered GraphQL Endpoints\n");

        foreach (var kvp in endpoints)
        {
            var endpointName = kvp.Key;
            var endpointInfo = kvp.Value;
            
            result.AppendLine($"## {endpointName}");
            result.AppendLine($"**URL:** {endpointInfo.Url}");
            result.AppendLine($"**Allow Mutations:** {(endpointInfo.AllowMutations ? "Yes" : "No")}");
            result.AppendLine($"**Tool Prefix:** {(string.IsNullOrEmpty(endpointInfo.ToolPrefix) ? "(none)" : endpointInfo.ToolPrefix)}");
            
            if (endpointInfo.Headers.Count > 0)
            {
                result.AppendLine($"**Headers:** {endpointInfo.Headers.Count} configured");
                foreach (var header in endpointInfo.Headers)
                {
                    result.AppendLine($"  - {header.Key}: {header.Value}");
                }
            }
            else
            {
                result.AppendLine("**Headers:** None");
            }

            // Count tools for this endpoint using the registry service
            var toolCount = Registry.GetToolCountForEndpoint(endpointName);
            result.AppendLine($"**Generated Tools:** {toolCount}");
            result.AppendLine();
        }

        return result.ToString();
    }

    /// <summary>
    /// Get endpoint information for a registered endpoint
    /// </summary>
    /// <param name="endpointName">Name of the endpoint</param>
    /// <returns>Endpoint information if found, null otherwise</returns>
    public static GraphQLEndpointInfo? GetEndpointInfo(string endpointName)
    {
        return Registry.GetEndpointInfo(endpointName);
    }

    /// <summary>
    /// Check if an endpoint is registered
    /// </summary>
    /// <param name="endpointName">Name of the endpoint</param>
    /// <returns>True if endpoint is registered, false otherwise</returns>
    public static bool IsEndpointRegistered(string endpointName)
    {
        return Registry.IsEndpointRegistered(endpointName);
    }

    /// <summary>
    /// Get all registered endpoint names
    /// </summary>
    /// <returns>Collection of endpoint names</returns>
    public static IEnumerable<string> GetRegisteredEndpointNames()
    {
        return Registry.GetRegisteredEndpointNames();
    }

    private static async Task<string> GenerateToolsFromSchema(GraphQLEndpointInfo endpointInfo)
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

            // Register the tool using the singleton service
            Registry.RegisterDynamicTool(toolName, toolInfo);
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
}
