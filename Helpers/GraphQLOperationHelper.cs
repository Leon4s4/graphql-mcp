using System.Text;
using System.Text.Json;
using Graphql.Mcp.Helpers;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper class for GraphQL operation generation and formatting
/// </summary>
public static class GraphQLOperationHelper
{
    /// <summary>
    /// Generates a GraphQL operation string
    /// </summary>
    public static string GenerateOperationString(JsonElement field, string operationType, string fieldName)
    {
        var operation = new StringBuilder();

        operation.AppendLine($"{operationType.ToLower()} {operationType}_{fieldName}(");

        if (field.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Array)
        {
            var parameters = new List<string>();
            foreach (var arg in args.EnumerateArray())
            {
                if (arg.TryGetProperty("name", out var argName) &&
                    arg.TryGetProperty("type", out var argType))
                {
                    var paramName = argName.GetString() ?? "";
                    var paramType = GraphQlTypeHelpers.GetTypeName(argType);
                    parameters.Add($"${paramName}: {paramType}");
                }
            }

            if (parameters.Count > 0)
            {
                operation.AppendLine(string.Join(",\n  ", parameters));
            }
        }

        operation.AppendLine(") {");

        operation.Append($"  {fieldName}");

        if (field.TryGetProperty("args", out var fieldArgs) && fieldArgs.ValueKind == JsonValueKind.Array)
        {
            var argList = new List<string>();
            foreach (var arg in fieldArgs.EnumerateArray())
            {
                if (arg.TryGetProperty("name", out var argName))
                {
                    var paramName = argName.GetString() ?? "";
                    argList.Add($"{paramName}: ${paramName}");
                }
            }

            if (argList.Count > 0)
            {
                operation.Append($"({string.Join(", ", argList)})");
            }
        }

        operation.AppendLine(" {");
        operation.AppendLine("    # Add your field selections here");
        operation.AppendLine("    # This is a template - customize the fields you need");
        operation.AppendLine("  }");
        operation.AppendLine("}");

        return operation.ToString();
    }

    /// <summary>
    /// Gets a description for a GraphQL field
    /// </summary>
    public static string GetFieldDescription(JsonElement field, string operationType, string fieldName)
    {
        var description = new StringBuilder();

        if (field.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
        {
            description.Append(desc.GetString());
        }
        else
        {
            description.Append($"Execute {operationType.ToLower()} operation: {fieldName}");
        }

        if (field.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Array)
        {
            var argCount = args.GetArrayLength();
            if (argCount > 0)
            {
                description.Append($" (requires {argCount} parameter{(argCount == 1 ? "" : "s")})");
            }
        }

        return description.ToString();
    }

    /// <summary>
    /// Formats a GraphQL response for display
    /// </summary>
    public static string FormatGraphQlResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return responseContent;
        }
    }
}
