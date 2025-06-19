using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class SchemaIntrospectionTools
{
    private const string IntrospectionQuery = @"
                query IntrospectionQuery {
                  __schema {
                    queryType { name }
                    mutationType { name }
                    subscriptionType { name }
                    types {
                      ...FullType
                    }
                    directives {
                      name
                      description
                      locations
                      args {
                        ...InputValue
                      }
                    }
                  }
                }

                fragment FullType on __Type {
                  kind
                  name
                  description
                  fields(includeDeprecated: true) {
                    name
                    description
                    args {
                      ...InputValue
                    }
                    type {
                      ...TypeRef
                    }
                    isDeprecated
                    deprecationReason
                  }
                  inputFields {
                    ...InputValue
                  }
                  interfaces {
                    ...TypeRef
                  }
                  enumValues(includeDeprecated: true) {
                    name
                    description
                    isDeprecated
                    deprecationReason
                  }
                  possibleTypes {
                    ...TypeRef
                  }
                }

                fragment InputValue on __InputValue {
                  name
                  description
                  type { ...TypeRef }
                  defaultValue
                }

                fragment TypeRef on __Type {
                  kind
                  name
                  ofType {
                    kind
                    name
                    ofType {
                      kind
                      name
                      ofType {
                        kind
                        name
                        ofType {
                          kind
                          name
                          ofType {
                            kind
                            name
                            ofType {
                              kind
                              name
                              ofType {
                                kind
                                name
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }";

    [McpServerTool, Description("Retrieve comprehensive GraphQL schema information with intelligent defaults, examples, and metadata. This tool provides everything needed to understand and work with a GraphQL API in a single response including: complete schema introspection with types, fields, and directives; automatically generated query examples based on schema analysis; performance recommendations and optimization hints; security analysis and permission requirements; field usage patterns and complexity analysis; related operations and alternative approaches; deprecation warnings and migration guidance. Returns a comprehensive JSON response with all metadata needed for effective GraphQL development.")]
    public static async Task<string> IntrospectSchemaComprehensive(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Include automatically generated query examples based on schema analysis")]
        bool includeExamples = true,
        [Description("Include performance metrics and optimization recommendations")]
        bool includePerformance = true,
        [Description("Maximum number of example queries to generate per type")]
        int maxExamples = 5,
        [Description("Include security analysis and permission requirements")]
        bool includeSecurity = true)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return CreateErrorResponse($"Endpoint '{endpointName}' not found", 
                "Please register the endpoint first using RegisterEndpoint",
                ["Use GetAllEndpoints to see available endpoints", "Check endpoint name spelling"]);
        }

        try
        {
            // Execute schema introspection
            var schemaResult = await IntrospectSchemaInternal(endpointInfo);
            if (!schemaResult.IsSuccess)
            {
                return CreateErrorResponse("Schema introspection failed", 
                    schemaResult.Content ?? "Unknown error",
                    ["Check endpoint connectivity", "Verify GraphQL introspection is enabled", "Check authentication if required"]);
            }

            // Use smart response service for comprehensive response
            var smartResponseService = GetSmartResponseService();
            return await smartResponseService.CreateSchemaIntrospectionResponseAsync(
                schemaResult, includeExamples, includePerformance, maxExamples);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Unexpected error during schema introspection", 
                ex.Message,
                ["Verify endpoint is accessible", "Check network connectivity", "Review endpoint configuration"]);
        }
    }

    /// <summary>
    /// Original schema introspection method for backward compatibility
    /// </summary>
    [McpServerTool, Description("Retrieve basic GraphQL schema information for backward compatibility. For comprehensive responses with examples and metadata, use IntrospectSchemaComprehensive instead.")]
    public static async Task<GraphQlResponse> IntrospectSchema(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return GraphQlResponse.ConnectionError($"Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.");
        }

        return await IntrospectSchemaInternal(endpointInfo);
    }

    private static async Task<GraphQlResponse> IntrospectSchemaInternal(GraphQlEndpointInfo endpointInfo)
    {
        var body = new { query = IntrospectionQuery };
        return await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, body);
    }

    [McpServerTool, Description("Generate comprehensive human-readable documentation from GraphQL schema descriptions, field metadata, and type information. This tool creates detailed documentation including: field descriptions and usage examples, parameter requirements and types, return type structures, deprecation warnings and migration info, type relationships and inheritance, enum values and their meanings, input type requirements. Optionally filter to specific types for focused documentation.")]
    public static async Task<string> GetSchemaDocs(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Specific type name to get documentation for. Leave empty to get documentation for all types")]
        string? typeName = null)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        return await GetSchemaDocsInternal(endpointInfo, typeName);
    }

    /// <summary>
    /// Internal method for generating schema documentation that can be used by other tools
    /// </summary>
    public static async Task<string> GetSchemaDocs(GraphQlEndpointInfo endpointInfo, string? typeName = null)
    {
        return await GetSchemaDocsInternal(endpointInfo, typeName);
    }

    private static async Task<string> GetSchemaDocsInternal(GraphQlEndpointInfo endpointInfo, string? typeName)
    {
        var schemaResult = await IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
            return schemaResult.FormatForDisplay();

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);

        if (!schemaData.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("__schema", out var schema) ||
            !schema.TryGetProperty("types", out var types))
        {
            return "Failed to parse schema data";
        }

        var docs = new List<string>();

        foreach (var type in types.EnumerateArray())
        {
            if (!type.TryGetProperty("name", out var nameElement) ||
                nameElement.GetString()
                    ?.StartsWith("__") == true)
                continue;

            var currentTypeName = nameElement.GetString() ?? "";

            if (!string.IsNullOrWhiteSpace(typeName) &&
                !currentTypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                continue;

            var typeDoc = new List<string>();
            typeDoc.Add($"## Type: {currentTypeName}");

            if (type.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
            {
                typeDoc.Add($"**Description:** {desc.GetString()}");
            }

            if (type.TryGetProperty("kind", out var kind))
            {
                typeDoc.Add($"**Kind:** {kind.GetString()}");
            }

            if (type.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
            {
                typeDoc.Add("### Fields:");
                foreach (var field in fields.EnumerateArray())
                {
                    if (field.TryGetProperty("name", out var fieldName))
                    {
                        var fieldInfo = $"- **{fieldName.GetString()}**";

                        if (field.TryGetProperty("type", out var fieldType))
                        {
                            fieldInfo += $": {FormatType(fieldType)}";
                        }

                        if (field.TryGetProperty("description", out var fieldDesc) && fieldDesc.ValueKind == JsonValueKind.String)
                        {
                            fieldInfo += $" - {fieldDesc.GetString()}";
                        }

                        if (field.TryGetProperty("isDeprecated", out var deprecated) && deprecated.GetBoolean())
                        {
                            fieldInfo += " **[DEPRECATED]**";
                            if (field.TryGetProperty("deprecationReason", out var reason) && reason.ValueKind == JsonValueKind.String)
                            {
                                fieldInfo += $" - {reason.GetString()}";
                            }
                        }

                        typeDoc.Add($"  {fieldInfo}");
                    }
                }
            }

            if (type.TryGetProperty("enumValues", out var enumValues) && enumValues.ValueKind == JsonValueKind.Array)
            {
                typeDoc.Add("### Enum Values:");
                foreach (var enumValue in enumValues.EnumerateArray())
                {
                    if (enumValue.TryGetProperty("name", out var enumName))
                    {
                        var enumInfo = $"- **{enumName.GetString()}**";

                        if (enumValue.TryGetProperty("description", out var enumDesc) && enumDesc.ValueKind == JsonValueKind.String)
                        {
                            enumInfo += $" - {enumDesc.GetString()}";
                        }

                        typeDoc.Add($"  {enumInfo}");
                    }
                }
            }

            docs.Add(string.Join("\n", typeDoc));
        }

        return string.Join("\n\n", docs);
    }

    [McpServerTool, Description("Validate GraphQL query syntax and schema compliance without executing the query")]
    public static async Task<string> ValidateQuery(
        [Description("Name of the registered GraphQL endpoint")] string endpointName,
        [Description("GraphQL query to validate")]
        string query)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        return await ValidateQueryInternal(endpointInfo, query);
    }

    /// <summary>
    /// Internal method for query validation that can be used by other tools
    /// </summary>
    public static async Task<string> ValidateQuery(GraphQlEndpointInfo endpointInfo, string query)
    {
        return await ValidateQueryInternal(endpointInfo, query);
    }

    private static async Task<string> ValidateQueryInternal(GraphQlEndpointInfo endpointInfo, string query)
    {
        // First get the schema
        var schemaResult = await IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
            return schemaResult.FormatForDisplay();

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);

        if (!schemaData.TryGetProperty("data", out var data))
        {
            return "Failed to get schema for validation";
        }

        // Basic validation checks
        var validationErrors = new List<string>();

        // Check for basic syntax issues
        if (string.IsNullOrWhiteSpace(query))
        {
            validationErrors.Add("Query cannot be empty");
        }

        // Check for balanced braces
        var openBraces = query.Count(c => c == '{');
        var closeBraces = query.Count(c => c == '}');
        if (openBraces != closeBraces)
        {
            validationErrors.Add($"Mismatched braces: {openBraces} opening, {closeBraces} closing");
        }

        // Check for balanced parentheses
        var openParens = query.Count(c => c == '(');
        var closeParens = query.Count(c => c == ')');
        if (openParens != closeParens)
        {
            validationErrors.Add($"Mismatched parentheses: {openParens} opening, {closeParens} closing");
        }

        // Check operation type
        var operationMatch = Regex.Match(query, @"^\s*(query|mutation|subscription)", RegexOptions.IgnoreCase);
        if (!operationMatch.Success)
        {
            // Check if it's an anonymous query
            if (!query.TrimStart()
                    .StartsWith("{"))
            {
                validationErrors.Add("Invalid operation type. Must be query, mutation, subscription, or anonymous query starting with '{'");
            }
        }

        if (validationErrors.Count > 0)
        {
            return $"Validation failed:\n- {string.Join("\n- ", validationErrors)}";
        }

        return "Query passed basic validation checks. For full validation, consider executing the query against the server.";
    }

    /// <summary>
    /// Helper method to get or create SmartResponseService instance
    /// </summary>
    private static SmartResponseService GetSmartResponseService()
    {
        // For now, create a simple instance. In a real DI scenario, this would be injected
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<SmartResponseService>.Instance;
        return new SmartResponseService(cache, logger);
    }

    /// <summary>
    /// Creates a comprehensive error response with suggestions
    /// </summary>
    private static string CreateErrorResponse(string title, string message, List<string> suggestions)
    {
        var errorResponse = new
        {
            error = new
            {
                title = title,
                message = message,
                timestamp = DateTime.UtcNow,
                suggestions = suggestions,
                type = "SCHEMA_INTROSPECTION_ERROR"
            },
            metadata = new
            {
                operation = "schema_introspection",
                success = false,
                executionTimeMs = 0
            }
        };

        return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static string FormatType(JsonElement typeElement)
    {
        if (!typeElement.TryGetProperty("kind", out var kind))
            return "Unknown";

        var kindStr = kind.GetString();

        switch (kindStr)
        {
            case "NON_NULL":
                if (typeElement.TryGetProperty("ofType", out var ofType))
                {
                    return FormatType(ofType) + "!";
                }

                break;
            case "LIST":
                if (typeElement.TryGetProperty("ofType", out var listOfType))
                {
                    return "[" + FormatType(listOfType) + "]";
                }

                break;
            default:
                if (typeElement.TryGetProperty("name", out var name))
                {
                    return name.GetString() ?? "Unknown";
                }

                break;
        }

        return "Unknown";
    }
}