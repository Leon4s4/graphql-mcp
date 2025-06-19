using System.Text;
using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Tools;
using HotChocolate.Language;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper class for GraphQL schema operations
/// </summary>
public static class GraphQlSchemaHelper
{
    private static readonly StrawberryShakeSchemaService _schemaService = new();
    /// <summary>
    /// Generates tools from a GraphQL schema
    /// </summary>
    public static async Task<string> GenerateToolsFromSchema(GraphQlEndpointInfo endpointInfo)
    {
        var schemaResult = await _schemaService.GetSchemaAsync(endpointInfo);
        if (!schemaResult.IsSuccess)
            return $"Failed to retrieve schema: {schemaResult.ErrorMessage}";

        var schema = schemaResult.Schema!;
        var rootTypes = _schemaService.GetRootTypes(schema);
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
        var queryType = _schemaService.FindTypeDefinition<ObjectTypeDefinitionNode>(schema, rootTypes.QueryType);
        if (queryType != null)
        {
            var queryToolsCount = GraphQLToolGenerator.GenerateToolsForType(queryType, "Query", endpointInfo);
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
            var mutationType = _schemaService.FindTypeDefinition<ObjectTypeDefinitionNode>(schema, rootTypes.MutationType);
            if (mutationType != null)
            {
                var mutationToolsCount = GraphQLToolGenerator.GenerateToolsForType(mutationType, "Mutation", endpointInfo);
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
        var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
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
