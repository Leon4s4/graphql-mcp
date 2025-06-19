using System.ComponentModel;
using System.Text.Json;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// MCP tool for executing GraphQL queries against registered endpoints
/// </summary>
[McpServerToolType]
public static class QueryGraphQlMcpTool
{
    [McpServerTool, Description(@"Execute GraphQL queries and mutations against registered endpoints with comprehensive error handling and formatted results.

This is the primary tool for executing GraphQL operations. It supports:
- Complex queries with nested selections
- Mutations (if enabled on endpoint)
- Variable substitution with type validation
- Automatic error handling and formatting
- Support for multiple registered endpoints

Examples:
- Query: 'query { users { id name email } }'
- Mutation: 'mutation($input: UserInput!) { createUser(input: $input) { id name } }'
- With variables: '{""name"": ""John"", ""email"": ""john@example.com""}'

Operation Types Supported:
- Query operations (data retrieval)
- Mutation operations (data modification, if allowed)
- Subscription operations (real-time updates, if supported)

Error Handling:
- Validates endpoint registration
- Checks mutation permissions
- Parses and validates variables JSON
- Returns formatted error messages with context")]
    public static async Task<string> QueryGraphQl(
        [Description("GraphQL query or mutation string. Examples: 'query { users { id name } }' or 'mutation($input: UserInput!) { createUser(input: $input) { id } }'")]
        string query,
        [Description("Name of the registered GraphQL endpoint. Use ListDynamicTools or GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Variables for the query as JSON object. Example: '{\"id\": 123, \"input\": {\"name\": \"John\", \"email\": \"john@example.com\"}}'")]
        string? variables = null)
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
            var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, request);
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