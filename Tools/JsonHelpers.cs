using System.Text.Json;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Utility helpers for converting <see cref="JsonElement"/> values
/// into CLR types.
/// </summary>
public static class JsonHelpers
{
    /// <summary>
    /// Convert a <see cref="JsonElement"/> object into a dictionary representation.
    /// </summary>
    public static Dictionary<string, object> JsonElementToDictionary(JsonElement element)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = JsonElementToObject(property.Value);
        }
        return dictionary;
    }

    /// <summary>
    /// Convert a <see cref="JsonElement"/> to a corresponding CLR object.
    /// </summary>
    public static object JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToArray(),
            JsonValueKind.Object => JsonElementToDictionary(element),
            _ => element.ToString()
        };
    }
}
