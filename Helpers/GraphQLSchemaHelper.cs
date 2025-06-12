using System.Text;
using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Tools;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper class for GraphQL schema operations
/// </summary>
public static class GraphQLSchemaHelper
{
    /// <summary>
    /// Generates tools from a GraphQL schema
    /// </summary>
    public static async Task<string> GenerateToolsFromSchema(GraphQlEndpointInfo endpointInfo)
    {
        var headersJson = endpointInfo.Headers.Count > 0
            ? JsonSerializer.Serialize(endpointInfo.Headers)
            : null;

        var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo.Url, headersJson);
        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

        if (!schemaData.TryGetProperty("data", out var data) || !data.TryGetProperty("__schema", out var schema))
        {
            return "Failed to parse schema introspection data";
        }

        var toolsGenerated = 0;

        if (schema.TryGetProperty("queryType", out var queryTypeRef) &&
            queryTypeRef.TryGetProperty("name", out var queryTypeName))
        {
            var queryType = FindTypeByName(schema, queryTypeName.GetString() ?? "Query");
            if (queryType.HasValue)
            {
                var queryTools = GraphQLToolGenerator.GenerateToolsForType(queryType.Value, "Query", endpointInfo);
                toolsGenerated += queryTools;
            }
        }

        if (endpointInfo.AllowMutations &&
            schema.TryGetProperty("mutationType", out var mutationTypeRef) &&
            mutationTypeRef.TryGetProperty("name", out var mutationTypeName))
        {
            var mutationType = FindTypeByName(schema, mutationTypeName.GetString() ?? "Mutation");
            if (mutationType.HasValue)
            {
                var mutationTools = GraphQLToolGenerator.GenerateToolsForType(mutationType.Value, "Mutation", endpointInfo);
                toolsGenerated += mutationTools;
            }
        }

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
}
