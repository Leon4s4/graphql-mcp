using System.Text;
using System.Text.Json;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;
using Graphql.Mcp.DTO;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Enhanced GraphQL schema service using StrawberryShake/HotChocolate for robust schema handling
/// </summary>
public class StrawberryShakeSchemaService
{
    private readonly Dictionary<string, DocumentNode> _cachedSchemas = new();

    /// <summary>
    /// Downloads and parses a GraphQL schema using introspection
    /// </summary>
    public async Task<SchemaResult> GetSchemaAsync(GraphQlEndpointInfo endpointInfo)
    {
        try
        {
            // Check cache first
            if (_cachedSchemas.TryGetValue(endpointInfo.Name, out var cachedSchema))
            {
                return SchemaResult.Success(cachedSchema);
            }

            // Perform introspection query
            var introspectionQuery = GetIntrospectionQuery();
            var requestBody = new { query = introspectionQuery };

            var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, requestBody);
            if (!result.IsSuccess)
            {
                return SchemaResult.Error($"Failed to introspect schema: {result.ErrorMessage}");
            }

            // Parse the introspection result
            JsonElement introspectionResult;
            try
            {
                introspectionResult = JsonSerializer.Deserialize<JsonElement>(result.Content!);
            }
            catch (JsonException ex)
            {
                return SchemaResult.Error($"Invalid JSON response from GraphQL endpoint: {ex.Message}");
            }

            if (!introspectionResult.TryGetProperty("data", out var data))
            {
                // Check if there are GraphQL errors
                if (introspectionResult.TryGetProperty("errors", out var errors))
                {
                    var errorMessages = new List<string>();
                    foreach (var error in errors.EnumerateArray())
                    {
                        if (error.TryGetProperty("message", out var errorMessage))
                        {
                            errorMessages.Add(errorMessage.GetString() ?? "Unknown error");
                        }
                    }
                    return SchemaResult.Error($"GraphQL errors in introspection response: {string.Join(", ", errorMessages)}");
                }
                return SchemaResult.Error("Invalid introspection response format - missing 'data' property");
            }

            // Check if data is null
            if (data.ValueKind == JsonValueKind.Null)
            {
                return SchemaResult.Error("Introspection returned null data - the endpoint may not support introspection or may require authentication");
            }

            // Convert introspection result to SDL (Schema Definition Language)
            string sdl;
            try
            {
                sdl = ConvertIntrospectionToSdl(data);
            }
            catch (Exception ex)
            {
                return SchemaResult.Error($"Error converting introspection result to schema: {ex.Message}");
            }
            
            // Parse SDL using HotChocolate
            DocumentNode document;
            try
            {
                document = Utf8GraphQLParser.Parse(sdl);
            }
            catch (Exception ex)
            {
                return SchemaResult.Error($"Error parsing generated schema SDL: {ex.Message}");
            }
            
            // Cache the parsed schema
            _cachedSchemas[endpointInfo.Name] = document;

            return SchemaResult.Success(document);
        }
        catch (Exception ex)
        {
            return SchemaResult.Error($"Unexpected error processing schema: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets type definitions from the schema
    /// </summary>
    public List<IDefinitionNode> GetTypeDefinitions(DocumentNode schema, TypeKind? typeKind = null)
    {
        var definitions = new List<IDefinitionNode>();

        foreach (var definition in schema.Definitions)
        {
            switch (definition)
            {
                case ObjectTypeDefinitionNode objectType when typeKind == null || typeKind == TypeKind.Object:
                    definitions.Add(objectType);
                    break;
                case InterfaceTypeDefinitionNode interfaceType when typeKind == null || typeKind == TypeKind.Interface:
                    definitions.Add(interfaceType);
                    break;
                case UnionTypeDefinitionNode unionType when typeKind == null || typeKind == TypeKind.Union:
                    definitions.Add(unionType);
                    break;
                case EnumTypeDefinitionNode enumType when typeKind == null || typeKind == TypeKind.Enum:
                    definitions.Add(enumType);
                    break;
                case InputObjectTypeDefinitionNode inputType when typeKind == null || typeKind == TypeKind.InputObject:
                    definitions.Add(inputType);
                    break;
                case ScalarTypeDefinitionNode scalarType when typeKind == null || typeKind == TypeKind.Scalar:
                    definitions.Add(scalarType);
                    break;
            }
        }

        return definitions;
    }

    /// <summary>
    /// Finds a specific type definition by name
    /// </summary>
    public T? FindTypeDefinition<T>(DocumentNode schema, string typeName) where T : class, IDefinitionNode
    {
        return schema.Definitions
            .OfType<T>()
            .FirstOrDefault(def => def switch
            {
                INamedSyntaxNode named => named.Name.Value == typeName,
                _ => false
            });
    }

    /// <summary>
    /// Gets the query, mutation, and subscription root types
    /// </summary>
    public RootTypes GetRootTypes(DocumentNode schema)
    {
        var schemaDefinition = schema.Definitions
            .OfType<SchemaDefinitionNode>()
            .FirstOrDefault();

        string queryType = "Query";
        string? mutationType = null;
        string? subscriptionType = null;

        if (schemaDefinition != null)
        {
            foreach (var operationType in schemaDefinition.OperationTypes)
            {
                switch (operationType.Operation)
                {
                    case OperationType.Query:
                        queryType = operationType.Type.Name.Value;
                        break;
                    case OperationType.Mutation:
                        mutationType = operationType.Type.Name.Value;
                        break;
                    case OperationType.Subscription:
                        subscriptionType = operationType.Type.Name.Value;
                        break;
                }
            }
        }

        return new RootTypes(queryType, mutationType, subscriptionType);
    }

    /// <summary>
    /// Compares two schemas and returns differences
    /// </summary>
    public async Task<SchemaComparison> CompareSchemas(GraphQlEndpointInfo endpoint1, GraphQlEndpointInfo endpoint2)
    {
        var schema1Result = await GetSchemaAsync(endpoint1);
        var schema2Result = await GetSchemaAsync(endpoint2);

        if (!schema1Result.IsSuccess)
            return SchemaComparison.Error($"Failed to get schema for {endpoint1.Name}: {schema1Result.ErrorMessage}");

        if (!schema2Result.IsSuccess)
            return SchemaComparison.Error($"Failed to get schema for {endpoint2.Name}: {schema2Result.ErrorMessage}");

        var differences = new List<SchemaDifference>();
        
        // Compare type definitions
        var types1 = GetTypeDefinitions(schema1Result.Schema!);
        var types2 = GetTypeDefinitions(schema2Result.Schema!);

        var typeNames1 = types1.OfType<INamedSyntaxNode>().Select(t => t.Name.Value).ToHashSet();
        var typeNames2 = types2.OfType<INamedSyntaxNode>().Select(t => t.Name.Value).ToHashSet();

        // Find added and removed types
        foreach (var typeName in typeNames2.Except(typeNames1))
        {
            differences.Add(new SchemaDifference(DifferenceType.TypeAdded, typeName, null, "Type added in schema 2"));
        }

        foreach (var typeName in typeNames1.Except(typeNames2))
        {
            differences.Add(new SchemaDifference(DifferenceType.TypeRemoved, typeName, null, "Type removed in schema 2"));
        }

        // Compare common types (simplified for now)
        foreach (var commonType in typeNames1.Intersect(typeNames2))
        {
            // Detailed field comparison would go here
            // This is a simplified version - you can expand this based on your needs
        }

        return SchemaComparison.Success(differences);
    }

    /// <summary>
    /// Formats a schema type for display
    /// </summary>
    public string FormatTypeDefinition(IDefinitionNode definition)
    {
        var result = new StringBuilder();

        switch (definition)
        {
            case ObjectTypeDefinitionNode objectType:
                result.AppendLine($"### {objectType.Name.Value} (Object)");
                if (objectType.Description != null)
                {
                    result.AppendLine($"*{objectType.Description.Value}*\n");
                }
                
                if (objectType.Fields.Any())
                {
                    result.AppendLine("**Fields:**");
                    foreach (var field in objectType.Fields)
                    {
                        var fieldType = GetTypeName(field.Type);
                        var description = field.Description?.Value ?? "";
                        result.AppendLine($"- `{field.Name.Value}`: {fieldType}" + 
                                        (!string.IsNullOrEmpty(description) ? $" - {description}" : ""));
                    }
                }
                break;

            case EnumTypeDefinitionNode enumType:
                result.AppendLine($"### {enumType.Name.Value} (Enum)");
                if (enumType.Description != null)
                {
                    result.AppendLine($"*{enumType.Description.Value}*\n");
                }
                
                if (enumType.Values.Any())
                {
                    result.AppendLine("**Values:**");
                    foreach (var value in enumType.Values)
                    {
                        var description = value.Description?.Value ?? "";
                        result.AppendLine($"- `{value.Name.Value}`" + 
                                        (!string.IsNullOrEmpty(description) ? $" - {description}" : ""));
                    }
                }
                break;

            // Add other type definitions as needed
        }

        return result.ToString();
    }

    private string GetTypeName(ITypeNode type)
    {
        return type switch
        {
            NonNullTypeNode nonNull => GetTypeName(nonNull.Type) + "!",
            ListTypeNode list => "[" + GetTypeName(list.Type) + "]",
            NamedTypeNode named => named.Name.Value,
            _ => "Unknown"
        };
    }

    private string ConvertIntrospectionToSdl(JsonElement introspectionData)
    {
        var sdl = new StringBuilder();
        
        if (!introspectionData.TryGetProperty("__schema", out var schema))
            throw new InvalidOperationException("Invalid introspection result - missing '__schema' property");

        // Check if schema is null
        if (schema.ValueKind == JsonValueKind.Null)
            throw new InvalidOperationException("Schema data is null - the endpoint may not support introspection");

        // Add schema definition if we have explicit root types
        if (schema.TryGetProperty("queryType", out var queryType) &&
            queryType.ValueKind != JsonValueKind.Null &&
            queryType.TryGetProperty("name", out var queryTypeName) &&
            queryTypeName.ValueKind != JsonValueKind.Null)
        {
            var schemaDefParts = new List<string>();
            var queryName = queryTypeName.GetString();
            if (!string.IsNullOrEmpty(queryName))
            {
                schemaDefParts.Add($"query: {queryName}");
            }
            
            if (schema.TryGetProperty("mutationType", out var mutationType) &&
                mutationType.ValueKind != JsonValueKind.Null &&
                mutationType.TryGetProperty("name", out var mutationTypeName) &&
                mutationTypeName.ValueKind != JsonValueKind.Null)
            {
                var mutationName = mutationTypeName.GetString();
                if (!string.IsNullOrEmpty(mutationName))
                {
                    schemaDefParts.Add($"mutation: {mutationName}");
                }
            }
            
            if (schema.TryGetProperty("subscriptionType", out var subscriptionType) &&
                subscriptionType.ValueKind != JsonValueKind.Null &&
                subscriptionType.TryGetProperty("name", out var subscriptionTypeName) &&
                subscriptionTypeName.ValueKind != JsonValueKind.Null)
            {
                var subscriptionName = subscriptionTypeName.GetString();
                if (!string.IsNullOrEmpty(subscriptionName))
                {
                    schemaDefParts.Add($"subscription: {subscriptionName}");
                }
            }
            
            if (schemaDefParts.Count > 0)
            {
                sdl.AppendLine("schema {");
                foreach (var part in schemaDefParts)
                {
                    sdl.AppendLine($"  {part}");
                }
                sdl.AppendLine("}");
                sdl.AppendLine();
            }
        }

        // Add type definitions
        if (schema.TryGetProperty("types", out var types) && types.ValueKind != JsonValueKind.Null)
        {
            foreach (var type in types.EnumerateArray())
            {
                if (!type.TryGetProperty("name", out var nameElement) || 
                    nameElement.ValueKind == JsonValueKind.Null ||
                    nameElement.GetString()?.StartsWith("__") == true)
                    continue;

                var typeName = nameElement.GetString();
                if (string.IsNullOrEmpty(typeName))
                    continue;

                if (!type.TryGetProperty("kind", out var kindElement) ||
                    kindElement.ValueKind == JsonValueKind.Null)
                    continue;

                var kind = kindElement.GetString();
                if (string.IsNullOrEmpty(kind))
                    continue;

                var description = type.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null ? desc.GetString() : null;

                try
                {
                    switch (kind)
                    {
                        case "OBJECT":
                            sdl.AppendLine(ConvertObjectType(type));
                            break;
                        case "ENUM":
                            sdl.AppendLine(ConvertEnumType(type));
                            break;
                        case "SCALAR":
                            // Only add custom scalars, not built-in ones
                            if (!string.IsNullOrEmpty(typeName) && 
                                !new[] { "String", "Int", "Float", "Boolean", "ID" }.Contains(typeName))
                            {
                                sdl.AppendLine($"scalar {typeName}");
                            }
                            break;
                        case "INTERFACE":
                            // For now, just convert as object type - could be enhanced later
                            sdl.AppendLine(ConvertObjectType(type));
                            break;
                        case "UNION":
                            // Simple union conversion
                            if (type.TryGetProperty("possibleTypes", out var possibleTypes) &&
                                possibleTypes.ValueKind != JsonValueKind.Null)
                            {
                                var typeNames = possibleTypes.EnumerateArray()
                                    .Select(t => t.TryGetProperty("name", out var n) && n.ValueKind != JsonValueKind.Null ? n.GetString() : null)
                                    .Where(n => !string.IsNullOrEmpty(n))
                                    .ToList();
                                
                                if (typeNames.Count > 0)
                                {
                                    sdl.AppendLine($"union {typeName} = {string.Join(" | ", typeNames)}");
                                }
                            }
                            break;
                        case "INPUT_OBJECT":
                            // Convert input object type
                            sdl.AppendLine(ConvertInputType(type));
                            break;
                        // Add other types as needed
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue processing other types
                    Console.WriteLine($"Error processing type {typeName}: {ex.Message}");
                }
            }
        }

        return sdl.ToString();
    }

    private string ConvertObjectType(JsonElement type)
    {
        var result = new StringBuilder();
        
        if (!type.TryGetProperty("name", out var nameElement) || 
            nameElement.ValueKind == JsonValueKind.Null)
            return "";
            
        var typeName = nameElement.GetString();
        if (string.IsNullOrEmpty(typeName))
            return "";
        
        if (type.TryGetProperty("description", out var desc) && 
            desc.ValueKind != JsonValueKind.Null && 
            !string.IsNullOrEmpty(desc.GetString()))
        {
            result.AppendLine($"\"\"\"{desc.GetString()}\"\"\"");
        }
        
        result.Append($"type {typeName}");
        
        // Add interfaces if any
        if (type.TryGetProperty("interfaces", out var interfaces) && 
            interfaces.ValueKind != JsonValueKind.Null && 
            interfaces.GetArrayLength() > 0)
        {
            var interfaceNames = new List<string>();
            foreach (var iface in interfaces.EnumerateArray())
            {
                if (iface.TryGetProperty("name", out var ifaceNameElement) &&
                    ifaceNameElement.ValueKind != JsonValueKind.Null)
                {
                    var ifaceName = ifaceNameElement.GetString();
                    if (!string.IsNullOrEmpty(ifaceName))
                    {
                        interfaceNames.Add(ifaceName);
                    }
                }
            }
            
            if (interfaceNames.Count > 0)
            {
                result.Append($" implements {string.Join(" & ", interfaceNames)}");
            }
        }
        
        result.AppendLine(" {");
        
        // Add fields
        if (type.TryGetProperty("fields", out var fields) && fields.ValueKind != JsonValueKind.Null)
        {
            foreach (var field in fields.EnumerateArray())
            {
                if (!field.TryGetProperty("name", out var fieldNameElement) ||
                    fieldNameElement.ValueKind == JsonValueKind.Null ||
                    !field.TryGetProperty("type", out var fieldTypeElement) ||
                    fieldTypeElement.ValueKind == JsonValueKind.Null)
                    continue;
                    
                var fieldName = fieldNameElement.GetString();
                if (string.IsNullOrEmpty(fieldName))
                    continue;
                    
                var fieldType = ConvertTypeReference(fieldTypeElement);
                
                if (field.TryGetProperty("description", out var fieldDesc) && 
                    fieldDesc.ValueKind != JsonValueKind.Null && 
                    !string.IsNullOrEmpty(fieldDesc.GetString()))
                {
                    result.AppendLine($"  \"\"\"{fieldDesc.GetString()}\"\"\"");
                }
                
                result.Append($"  {fieldName}");
                
                // Add arguments if any
                if (field.TryGetProperty("args", out var args) && 
                    args.ValueKind != JsonValueKind.Null && 
                    args.GetArrayLength() > 0)
                {
                    var argStrings = new List<string>();
                    foreach (var arg in args.EnumerateArray())
                    {
                        if (!arg.TryGetProperty("name", out var argNameElement) ||
                            argNameElement.ValueKind == JsonValueKind.Null ||
                            !arg.TryGetProperty("type", out var argTypeElement) ||
                            argTypeElement.ValueKind == JsonValueKind.Null)
                            continue;
                            
                        var argName = argNameElement.GetString();
                        if (string.IsNullOrEmpty(argName))
                            continue;
                            
                        var argType = ConvertTypeReference(argTypeElement);
                        argStrings.Add($"{argName}: {argType}");
                    }
                    if (argStrings.Count > 0)
                    {
                        result.Append($"({string.Join(", ", argStrings)})");
                    }
                }
                
                result.AppendLine($": {fieldType}");
            }
        }
        
        result.AppendLine("}");
        return result.ToString();
    }

    private string ConvertEnumType(JsonElement type)
    {
        var result = new StringBuilder();
        var typeName = type.GetProperty("name").GetString();
        
        if (type.TryGetProperty("description", out var desc) && !string.IsNullOrEmpty(desc.GetString()))
        {
            result.AppendLine($"\"\"\"{desc.GetString()}\"\"\"");
        }
        
        result.AppendLine($"enum {typeName} {{");
        
        if (type.TryGetProperty("enumValues", out var values))
        {
            foreach (var value in values.EnumerateArray())
            {
                var valueName = value.GetProperty("name").GetString();
                
                if (value.TryGetProperty("description", out var valueDesc) && !string.IsNullOrEmpty(valueDesc.GetString()))
                {
                    result.AppendLine($"  \"\"\"{valueDesc.GetString()}\"\"\"");
                }
                
                result.AppendLine($"  {valueName}");
            }
        }
        
        result.AppendLine("}");
        return result.ToString();
    }

    private string ConvertInputType(JsonElement type)
    {
        var result = new StringBuilder();
        var typeName = type.GetProperty("name").GetString();
        
        if (type.TryGetProperty("description", out var desc) && !string.IsNullOrEmpty(desc.GetString()))
        {
            result.AppendLine($"\"\"\"{desc.GetString()}\"\"\"");
        }
        
        result.AppendLine($"input {typeName} {{");
        
        if (type.TryGetProperty("inputFields", out var fields))
        {
            foreach (var field in fields.EnumerateArray())
            {
                var fieldName = field.GetProperty("name").GetString();
                var fieldType = ConvertTypeReference(field.GetProperty("type"));
                
                if (field.TryGetProperty("description", out var fieldDesc) && !string.IsNullOrEmpty(fieldDesc.GetString()))
                {
                    result.AppendLine($"  \"\"\"{fieldDesc.GetString()}\"\"\"");
                }
                
                result.AppendLine($"  {fieldName}: {fieldType}");
            }
        }
        
        result.AppendLine("}");
        return result.ToString();
    }

    private string ConvertTypeReference(JsonElement typeRef)
    {
        if (!typeRef.TryGetProperty("kind", out var kindElement))
            return "Unknown";
            
        var kind = kindElement.GetString();
        
        return kind switch
        {
            "NON_NULL" => typeRef.TryGetProperty("ofType", out var nonNullOfType) 
                ? ConvertTypeReference(nonNullOfType) + "!" 
                : "Unknown!",
            "LIST" => typeRef.TryGetProperty("ofType", out var listOfType) 
                ? "[" + ConvertTypeReference(listOfType) + "]" 
                : "[Unknown]",
            _ => typeRef.TryGetProperty("name", out var nameElement) 
                ? nameElement.GetString() ?? "Unknown" 
                : "Unknown"
        };
    }

    private static string GetIntrospectionQuery()
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

/// <summary>
/// Result wrapper for schema operations
/// </summary>
public class SchemaResult
{
    public bool IsSuccess { get; init; }
    public DocumentNode? Schema { get; init; }
    public string? ErrorMessage { get; init; }

    private SchemaResult() { }

    public static SchemaResult Success(DocumentNode schema) => new()
    {
        IsSuccess = true,
        Schema = schema
    };

    public static SchemaResult Error(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };
}

/// <summary>
/// Root types information
/// </summary>
public record RootTypes(string QueryType, string? MutationType, string? SubscriptionType);

/// <summary>
/// Schema comparison result
/// </summary>
public class SchemaComparison
{
    public bool IsSuccess { get; init; }
    public List<SchemaDifference> Differences { get; init; } = new();
    public string? ErrorMessage { get; init; }

    private SchemaComparison() { }

    public static SchemaComparison Success(List<SchemaDifference> differences) => new()
    {
        IsSuccess = true,
        Differences = differences
    };

    public static SchemaComparison Error(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };
}

/// <summary>
/// Schema difference information
/// </summary>
public record SchemaDifference(
    DifferenceType Type,
    string TypeName,
    string? FieldName,
    string Description);

/// <summary>
/// Type of schema difference
/// </summary>
public enum DifferenceType
{
    TypeAdded,
    TypeRemoved,
    FieldAdded,
    FieldRemoved,
    FieldTypeChanged
}

/// <summary>
/// GraphQL type kinds
/// </summary>
public enum TypeKind
{
    Object,
    Interface,
    Union,
    Enum,
    InputObject,
    Scalar
}
