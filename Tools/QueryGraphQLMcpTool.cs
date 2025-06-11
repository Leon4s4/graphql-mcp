using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace Tools;

/// <summary>
/// MCP tool for executing GraphQL queries against registered endpoints
/// </summary>
[McpServerToolType]
public static class QueryGraphQLMcpTool
{
    [McpServerTool, Description("Execute GraphQL queries and mutations against registered endpoints")]
    public static async Task<string> QueryGraphQL(
        [Description("GraphQL query or mutation to execute")] string query,
        [Description("Name of the registered endpoint (use ListDynamicTools to see available endpoints)")] string endpointName,
        [Description("Variables for the query as JSON object (optional)")] string? variables = null)
    {
        try
        {
            // Get the endpoint from the dynamic registry
            var endpointsField = typeof(DynamicToolRegistry).GetField("_endpoints", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            if (endpointsField?.GetValue(null) is not Dictionary<string, object> endpoints)
            {
                return "Error: Could not access endpoint registry. Please ensure endpoints are registered using RegisterEndpoint.";
            }

            if (!endpoints.ContainsKey(endpointName))
            {
                return $"Endpoint '{endpointName}' not found. Available endpoints: {string.Join(", ", endpoints.Keys)}. Use RegisterEndpoint to add new endpoints.";
            }

            // Use reflection to access the endpoint info
            var endpointInfo = endpoints[endpointName];
            var endpointType = endpointInfo.GetType();
            
            var urlProp = endpointType.GetProperty("Url");
            var headersProp = endpointType.GetProperty("Headers");
            var allowMutationsProp = endpointType.GetProperty("AllowMutations");

            if (urlProp?.GetValue(endpointInfo) is not string url)
            {
                return "Error: Could not retrieve endpoint URL.";
            }

            var headers = headersProp?.GetValue(endpointInfo) as Dictionary<string, string> ?? new();
            var allowMutations = allowMutationsProp?.GetValue(endpointInfo) as bool? ?? false;

            // Check if it's a mutation and if mutations are allowed
            if (IsMutation(query) && !allowMutations)
            {
                return $"Mutations are not allowed for endpoint '{endpointName}'. Enable mutations when registering the endpoint to use mutation operations.";
            }

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
            }

            using var client = HttpClientHelper.CreateStaticClient(headers);

            var request = new
            {
                query = query,
                variables = variableDict.Count > 0 ? variableDict : null
            };

            var content = HttpClientHelper.CreateGraphQLContent(request);
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return FormatErrorResponse($"HTTP Error {response.StatusCode}", responseContent);
            }

            // Parse and format the response
            try
            {
                using var responseDoc = JsonDocument.Parse(responseContent);
                var hasErrors = responseDoc.RootElement.TryGetProperty("errors", out var errors) && 
                               errors.ValueKind == JsonValueKind.Array && 
                               errors.GetArrayLength() > 0;

                if (hasErrors)
                {
                    return FormatErrorResponse("GraphQL Errors", responseContent);
                }

                return FormatSuccessResponse(responseContent);
            }
            catch (JsonException)
            {
                return FormatSuccessResponse(responseContent);
            }
        }
        catch (Exception ex)
        {
            return $"Error executing GraphQL query: {ex.Message}";
        }
    }

    private static bool IsMutation(string query)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(query, @"\bmutation\b", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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

    private static string FormatSuccessResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var formatted = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
            
            var result = new StringBuilder();
            result.AppendLine("# GraphQL Query Result\n");
            result.AppendLine("✅ **Status:** Success\n");
            result.AppendLine("## Response");
            result.AppendLine("```json");
            result.AppendLine(formatted);
            result.AppendLine("```");
            
            return result.ToString();
        }
        catch
        {
            return $"# GraphQL Query Result\n\n✅ **Status:** Success\n\n## Response\n{responseContent}";
        }
    }

    private static string FormatErrorResponse(string errorType, string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var formatted = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
            
            var result = new StringBuilder();
            result.AppendLine("# GraphQL Query Result\n");
            result.AppendLine($"❌ **Status:** {errorType}\n");
            result.AppendLine("## Error Details");
            result.AppendLine("```json");
            result.AppendLine(formatted);
            result.AppendLine("```");
            
            return result.ToString();
        }
        catch
        {
            return $"# GraphQL Query Result\n\n❌ **Status:** {errorType}\n\n## Error Details\n{responseContent}";
        }
    }
}
