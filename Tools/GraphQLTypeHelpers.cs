using System.Text.Json;

namespace Tools;

public static class GraphQLTypeHelpers
{
    public static string GetTypeName(JsonElement typeElement)
    {
        var kind = typeElement.GetProperty("kind").GetString();

        return kind switch
        {
            "NON_NULL" => GetTypeName(typeElement.GetProperty("ofType")) + "!",
            "LIST" => "[" + GetTypeName(typeElement.GetProperty("ofType")) + "]",
            _ => typeElement.TryGetProperty("name", out var name) ? name.GetString() ?? "Unknown" : "Unknown"
        };
    }

    public static string ConvertGraphQLTypeToCSharp(string graphqlType, bool useIEnumerable = false)
    {
        var isNonNull = graphqlType.EndsWith("!");
        var isList = graphqlType.Contains("[");
        var baseType = graphqlType.Replace("!", "").Replace("[", "").Replace("]", "");

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
}
