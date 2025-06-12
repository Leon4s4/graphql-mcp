using System.Text.Json;

namespace Graphql.Mcp.Helpers;

public static class GraphQlTypeHelpers
{
    public static string GetTypeName(JsonElement typeElement)
    {
        var kind = typeElement.GetProperty("kind")
            .GetString();

        return kind switch
        {
            "NON_NULL" => GetTypeName(typeElement.GetProperty("ofType")) + "!",
            "LIST" => "[" + GetTypeName(typeElement.GetProperty("ofType")) + "]",
            _ => typeElement.TryGetProperty("name", out var name) ? name.GetString() ?? "Unknown" : "Unknown"
        };
    }

    public static string ConvertGraphQlTypeToCSharp(string graphqlType, bool useIEnumerable = false)
    {
        var isNonNull = graphqlType.EndsWith("!");
        var isList = graphqlType.Contains("[");
        var baseType = graphqlType.Replace("!", "")
            .Replace("[", "")
            .Replace("]", "");

        var csharpType = baseType switch
        {
            "String" => "string",
            "Int" => "int",
            "Float" => "double",
            "Boolean" => "bool",
            "ID" => "string",
            _ => baseType
        };

        if (isList)
        {
            var collection = useIEnumerable ? "IEnumerable" : "List";
            csharpType = $"{collection}<{csharpType}>";
        }

        if (!isNonNull)
        {
            if (useIEnumerable)
            {
                if (!isList)
                    csharpType += "?";
            }
            else if (!isList && (csharpType == "int" || csharpType == "double" || csharpType == "bool"))
            {
                csharpType += "?";
            }
        }

        return csharpType;
    }

    /// <summary>
    /// Gets the named type name by unwrapping NonNull and List wrappers
    /// </summary>
    public static string GetNamedTypeName(JsonElement type)
    {
        // Unwrap NonNull and List types to get the actual type name
        var current = type;
        while (current.TryGetProperty("ofType", out var ofType) && ofType.ValueKind != JsonValueKind.Null)
        {
            current = ofType;
        }

        if (current.TryGetProperty("name", out var name))
        {
            return name.GetString() ?? "";
        }

        return "";
    }

    /// <summary>
    /// Checks if a type name represents a scalar type
    /// </summary>
    public static bool IsScalarType(string typeName)
    {
        var scalarTypes = new[]
        {
            "String", "Int", "Float", "Boolean", "ID",
            // Common custom scalars
            "DateTime", "Date", "Time", "JSON", "Upload", "Long", "Decimal"
        };
        return scalarTypes.Contains(typeName);
    }

    /// <summary>
    /// Finds a type by name in the schema
    /// </summary>
    public static JsonElement? FindTypeByName(JsonElement schema, string typeName)
    {
        if (!schema.TryGetProperty("types", out var types))
            return null;

        foreach (var type in types.EnumerateArray())
        {
            if (type.TryGetProperty("name", out var name) &&
                name.GetString() == typeName)
            {
                return type;
            }
        }

        return null;
    }
}