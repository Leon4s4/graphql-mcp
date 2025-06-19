using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Graphql.Mcp.DTO;
using HotChocolate.Language;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper class for GraphQL schema operations
/// </summary>
public static class GraphQlSchemaHelper
{
    private static readonly StrawberryShakeSchemaService SchemaService = new();

    /// <summary>
    /// Generates tools from a GraphQL schema
    /// </summary>
    public static async Task<string> GenerateToolsFromSchema(GraphQlEndpointInfo endpointInfo)
    {
        var schemaResult = await SchemaService.GetSchemaAsync(endpointInfo);
        if (!schemaResult.IsSuccess)
            return $"Failed to retrieve schema: {schemaResult.ErrorMessage}";

        var schema = schemaResult.Schema!;
        var rootTypes = SchemaService.GetRootTypes(schema);
        var toolsGenerated = 0;

        // Debug information
        var debugInfo = new StringBuilder();
        debugInfo.AppendLine($"Schema root types detected:");
        debugInfo.AppendLine($"- Query: {rootTypes.QueryType}");
        debugInfo.AppendLine($"- Mutation: {rootTypes.MutationType ?? "None"}");
        debugInfo.AppendLine($"- Subscription: {rootTypes.SubscriptionType ?? "None"}");
        debugInfo.AppendLine($"- Allow Mutations: {endpointInfo.AllowMutations}");
        debugInfo.AppendLine();

        // Process Query type
        var queryType = SchemaService.FindTypeDefinition<ObjectTypeDefinitionNode>(schema, rootTypes.QueryType);
        if (queryType != null)
        {
            var queryToolsCount = GraphQlToolGenerator.GenerateToolsForType(queryType, "Query", endpointInfo);
            toolsGenerated += queryToolsCount;
            debugInfo.AppendLine($"Generated {queryToolsCount} query tools from {queryType.Fields.Count} fields");
        }
        else
        {
            debugInfo.AppendLine($"Warning: Could not find Query type '{rootTypes.QueryType}' in schema");
        }

        // Process Mutation type (only if mutations are allowed)
        if (endpointInfo.AllowMutations && !string.IsNullOrEmpty(rootTypes.MutationType))
        {
            var mutationType = SchemaService.FindTypeDefinition<ObjectTypeDefinitionNode>(schema, rootTypes.MutationType);
            if (mutationType != null)
            {
                var mutationToolsCount = GraphQlToolGenerator.GenerateToolsForType(mutationType, "Mutation", endpointInfo);
                toolsGenerated += mutationToolsCount;
                debugInfo.AppendLine($"Generated {mutationToolsCount} mutation tools from {mutationType.Fields.Count} fields");
            }
            else
            {
                debugInfo.AppendLine($"Warning: Could not find Mutation type '{rootTypes.MutationType}' in schema");
            }
        }
        else if (!endpointInfo.AllowMutations)
        {
            debugInfo.AppendLine("Mutations are disabled for this endpoint");
        }
        else
        {
            debugInfo.AppendLine("No mutation type found in schema");
        }

        debugInfo.AppendLine();
        debugInfo.AppendLine(GenerateResultMessage(toolsGenerated, endpointInfo));

        return debugInfo.ToString();
    }

    /// <summary>
    /// Retrieves and validates schema from GraphQL endpoint
    /// </summary>
    public static async Task<JsonElement> GetSchemaAsync(GraphQlEndpointInfo endpointInfo)
    {
        // Call IntrospectSchemaInternal directly since we have the endpoint info object
        var schemaResult = await IntrospectSchemaInternal(endpointInfo);
        if (!schemaResult.IsSuccess)
            throw new InvalidOperationException(schemaResult.FormatForDisplay());

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);

        if (!schemaData.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("__schema", out var schema))
        {
            throw new InvalidOperationException("Failed to retrieve schema data");
        }

        return schema;
    }

    private static int ProcessRootType(JsonElement schema, string typeRefName, string defaultTypeName, GraphQlEndpointInfo endpointInfo, bool shouldProcess)
    {
        if (!shouldProcess)
            return 0;

        if (!schema.TryGetProperty(typeRefName, out var typeRef) ||
            !typeRef.TryGetProperty("name", out var typeName))
            return 0;

        var rootType = FindTypeByName(schema, typeName.GetString() ?? defaultTypeName);
        if (!rootType.HasValue)
            return 0;

        return GraphQlToolGenerator.GenerateToolsForType(rootType.Value, defaultTypeName, endpointInfo);
    }

    private static string GenerateResultMessage(int toolsGenerated, GraphQlEndpointInfo endpointInfo)
    {
        var result = new StringBuilder();
        result.AppendLine($"Generated {toolsGenerated} dynamic tools for endpoint '{endpointInfo.Name}'");

        if (!endpointInfo.AllowMutations)
            result.AppendLine("Note: Mutations were not enabled for this endpoint");

        return result.ToString();
    }

    /// <summary>
    /// Finds a type by name in the GraphQL schema
    /// </summary>
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

    /// <summary>
    /// Finds an operation field in the schema by name
    /// </summary>
    public static JsonElement FindOperationField(JsonElement schema, string operationName)
    {
        // Find the Query type
        if (!schema.TryGetProperty("queryType", out var queryTypeRef) ||
            !queryTypeRef.TryGetProperty("name", out var queryTypeName))
        {
            throw new InvalidOperationException("No Query type found in schema");
        }

        var queryType = FindTypeByName(schema, queryTypeName.GetString() ?? "Query");
        if (!queryType.HasValue)
        {
            throw new InvalidOperationException("Query type not found in schema types");
        }

        // Find the specific operation field
        if (!queryType.Value.TryGetProperty("fields", out var fields))
        {
            throw new InvalidOperationException("No fields found in Query type");
        }

        foreach (var field in fields.EnumerateArray())
        {
            if (field.TryGetProperty("name", out var nameElement) &&
                nameElement.GetString() == operationName)
            {
                return field;
            }
        }

        throw new InvalidOperationException($"Operation '{operationName}' not found in Query type");
    }

    //TODO: leonardo
    /// <summary>
    /// Internal method to introspect schema from GraphQL endpoint
    /// </summary>
    public static async Task<GraphQlResponse> IntrospectSchemaInternal(GraphQlEndpointInfo endpointInfo)
    {
        using var httpClient = new HttpClient();

        // Add headers from endpoint info
        if (endpointInfo.Headers?.Count > 0)
        {
            foreach (var header in endpointInfo.Headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        var introspectionQuery = CreateIntrospectionQuery();

        try
        {
            var response = await httpClient.PostAsJsonAsync(endpointInfo.Url, new
            {
                query = introspectionQuery
            });

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return GraphQlResponse.Success(responseContent);
            }
            else
            {
                return GraphQlResponse.HttpError(response.StatusCode, response.ReasonPhrase ?? "Unknown error", responseContent);
            }
        }
        catch (Exception ex)
        {
            return GraphQlResponse.ConnectionError($"Connection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates the standard GraphQL introspection query
    /// </summary>
    private static string CreateIntrospectionQuery()
    {
        return @"
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
    }
}