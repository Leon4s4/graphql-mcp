using System.Text;
using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Tools;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper class for GraphQL schema operations
/// </summary>
public static class GraphQlSchemaHelper
{
    /// <summary>
    /// Generates tools from a GraphQL schema
    /// </summary>
    public static async Task<string> GenerateToolsFromSchema(GraphQlEndpointInfo endpointInfo)
    {
        var headersJson = endpointInfo.Headers.Count > 0
            ? JsonSerializer.Serialize(endpointInfo.Headers)
            : null;

        var schema = await GetSchemaFromEndpoint(endpointInfo.Url, headersJson);
        if (schema == null)
        {
            return "Failed to parse schema introspection data";
        }

        var toolsGenerated = 0;
        
        // Process Query type
        toolsGenerated += ProcessRootType(schema.Value, "queryType", "Query", endpointInfo, true);
        
        // Process Mutation type (only if mutations are allowed)
        toolsGenerated += ProcessRootType(schema.Value, "mutationType", "Mutation", endpointInfo, endpointInfo.AllowMutations);

        return GenerateResultMessage(toolsGenerated, endpointInfo);
    }

    /// <summary>
    /// Retrieves and validates schema from GraphQL endpoint
    /// </summary>
    public static async Task<JsonElement> GetSchemaAsync(string endpoint, string? headers)
    {
        var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

        if (!schemaData.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("__schema", out var schema))
        {
            throw new InvalidOperationException("Failed to retrieve schema data");
        }

        return schema;
    }

    private static async Task<JsonElement?> GetSchemaFromEndpoint(string url, string? headersJson)
    {
        var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(url, headersJson);
        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

        if (!schemaData.TryGetProperty("data", out var data) || !data.TryGetProperty("__schema", out var schema))
        {
            return null;
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

        return GraphQLToolGenerator.GenerateToolsForType(rootType.Value, defaultTypeName, endpointInfo);
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
}
