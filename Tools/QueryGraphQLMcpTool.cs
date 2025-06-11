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
            // Check if endpoint is registered using EndpointRegistryService
            if (!EndpointRegistryService.Instance.IsEndpointRegistered(endpointName))
            {
                var registeredEndpoints = EndpointRegistryService.Instance.GetRegisteredEndpointNames();
                return $"Endpoint '{endpointName}' not found. Available endpoints: {string.Join(", ", registeredEndpoints)}. Use RegisterEndpoint to add new endpoints.";
            }

            // Get endpoint information using EndpointRegistryService
            var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
            if (endpointInfo == null)
            {
                return "Error: Could not retrieve endpoint information.";
            }

            // Check if it's a mutation and if mutations are allowed
            if (IsMutation(query) && !endpointInfo.AllowMutations)
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
                    variableDict = JsonHelpers.JsonElementToDictionary(document.RootElement);
                }
                catch (JsonException ex)
                {
                    return $"Error parsing variables JSON: {ex.Message}";
                }
            }

            var request = new
            {
                query = query,
                variables = variableDict.Count > 0 ? variableDict : null
            };

            // Use centralized HTTP execution with proper error handling
            var result = await HttpClientHelper.ExecuteGraphQLRequestAsync(endpointInfo.Url, request, endpointInfo.Headers);
            return result.FormatForDisplay();
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

}
