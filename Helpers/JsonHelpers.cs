using System.Text.Json;

namespace Graphql.Mcp.Helpers;

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
    private static object JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            JsonValueKind.Array => element.EnumerateArray()
                .Select(JsonElementToObject)
                .ToArray(),
            JsonValueKind.Object => JsonElementToDictionary(element),
            _ => element.ToString()
        };
    }

    /// <summary>
    /// Parse a JSON string into a dictionary of string key-value pairs.
    /// Returns empty dictionary if input is null/empty.
    /// </summary>
    /// <param name="jsonString">JSON string to parse</param>
    /// <returns>Dictionary of headers or empty dictionary if input is null/empty</returns>
    public static (Dictionary<string, string> Headers, string? ErrorMessage) ParseHeadersJson(string? jsonString)
    {
        var headers = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(jsonString))
            return (headers, null);

        try
        {
            headers = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString) ?? new();
            return (headers, null);
        }
        catch (JsonException ex)
        {
            return (new Dictionary<string, string>(), $"Error parsing headers JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Parse GraphQL variables from JSON string into a dictionary
    /// </summary>
    /// <param name="variables">JSON string containing variables</param>
    /// <returns>Dictionary of parsed variables or empty dictionary if parsing fails</returns>
    public static Dictionary<string, object> ParseVariables(string variables)
    {
        try
        {
            if (string.IsNullOrEmpty(variables))
                return new Dictionary<string, object>();

            using var document = JsonDocument.Parse(variables);
            return JsonElementToDictionary(document.RootElement);
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }
}