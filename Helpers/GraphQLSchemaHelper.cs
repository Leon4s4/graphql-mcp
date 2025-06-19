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
    public static async Task<ToolGenerationResponse> GenerateToolsFromSchema(GraphQlEndpointInfo endpointInfo)
    {
        try
        {
            var schemaResult = await SchemaService.GetSchemaAsync(endpointInfo);
            if (!schemaResult.IsSuccess)
            {
                return ToolGenerationResponse.CreateError(
                    "SCHEMA_RETRIEVAL_FAILED", 
                    $"Failed to retrieve schema: {schemaResult.ErrorMessage}",
                    "Unable to connect to the GraphQL endpoint or retrieve its schema");
            }

            var schema = schemaResult.Schema!;
            var rootTypes = SchemaService.GetRootTypes(schema);

            var queryToolsCount = 0;
            var mutationToolsCount = 0;
            var queryFieldsCount = 0;
            var mutationFieldsCount = 0;
            var generatedTools = new List<GeneratedToolInfo>();

            // Process Query type
            var queryType = SchemaService.FindTypeDefinition<ObjectTypeDefinitionNode>(schema, rootTypes.QueryType);
            if (queryType != null)
            {
                queryFieldsCount = queryType.Fields.Count;
                queryToolsCount = GraphQlToolGenerator.GenerateToolsForType(queryType, "Query", endpointInfo);
                
                // Collect query tool information
                foreach (var field in queryType.Fields)
                {
                    generatedTools.Add(new GeneratedToolInfo
                    {
                        Name = $"{endpointInfo.ToolPrefix}{(string.IsNullOrEmpty(endpointInfo.ToolPrefix) ? "" : "_")}query_{field.Name.Value}",
                        Type = "query",
                        Description = field.Description?.Value ?? $"GraphQL query: {field.Name.Value}",
                        Parameters = field.Arguments.Select(arg => arg.Name.Value).ToList(),
                        IsDeprecated = field.Directives.Any(d => d.Name.Value == "deprecated"),
                        DeprecationReason = field.Directives
                            .FirstOrDefault(d => d.Name.Value == "deprecated")
                            ?.Arguments
                            ?.FirstOrDefault(a => a.Name.Value == "reason")
                            ?.Value.ToString()
                    });
                }
            }

            // Process Mutation type (only if mutations are allowed)
            if (endpointInfo.AllowMutations && !string.IsNullOrEmpty(rootTypes.MutationType))
            {
                var mutationType = SchemaService.FindTypeDefinition<ObjectTypeDefinitionNode>(schema, rootTypes.MutationType);
                if (mutationType != null)
                {
                    mutationFieldsCount = mutationType.Fields.Count;
                    mutationToolsCount = GraphQlToolGenerator.GenerateToolsForType(mutationType, "Mutation", endpointInfo);
                    
                    // Collect mutation tool information
                    foreach (var field in mutationType.Fields)
                    {
                        generatedTools.Add(new GeneratedToolInfo
                        {
                            Name = $"{endpointInfo.ToolPrefix}{(string.IsNullOrEmpty(endpointInfo.ToolPrefix) ? "" : "_")}mutation_{field.Name.Value}",
                            Type = "mutation",
                            Description = field.Description?.Value ?? $"GraphQL mutation: {field.Name.Value}",
                            Parameters = field.Arguments.Select(arg => arg.Name.Value).ToList(),
                            IsDeprecated = field.Directives.Any(d => d.Name.Value == "deprecated"),
                            DeprecationReason = field.Directives
                                .FirstOrDefault(d => d.Name.Value == "deprecated")
                                ?.Arguments
                                ?.FirstOrDefault(a => a.Name.Value == "reason")
                                ?.Value.ToString()
                        });
                    }
                }
            }

            var totalToolsGenerated = queryToolsCount + mutationToolsCount;

            var data = new ToolGenerationData
            {
                EndpointName = endpointInfo.Name,
                EndpointUrl = endpointInfo.Url,
                QueryType = rootTypes.QueryType,
                MutationType = rootTypes.MutationType,
                SubscriptionType = rootTypes.SubscriptionType,
                AllowMutations = endpointInfo.AllowMutations,
                QueryToolsGenerated = queryToolsCount,
                MutationToolsGenerated = mutationToolsCount,
                TotalToolsGenerated = totalToolsGenerated,
                QueryFieldsCount = queryFieldsCount,
                MutationFieldsCount = mutationFieldsCount,
                GeneratedTools = generatedTools,
                SchemaMetadata = new Dictionary<string, object>
                {
                    ["hasQuery"] = queryType != null,
                    ["hasMutation"] = !string.IsNullOrEmpty(rootTypes.MutationType),
                    ["hasSubscription"] = !string.IsNullOrEmpty(rootTypes.SubscriptionType),
                    ["totalTypes"] = schema.Definitions.Count
                }
            };

            return ToolGenerationResponse.CreateSuccess(data, 
                $"Successfully registered endpoint '{endpointInfo.Name}' with {totalToolsGenerated} dynamic tools.");
        }
        catch (Exception ex)
        {
            return ToolGenerationResponse.CreateError(
                "UNEXPECTED_ERROR", 
                $"Unexpected error during tool generation: {ex.Message}",
                "An unexpected error occurred while processing the GraphQL schema");
        }
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