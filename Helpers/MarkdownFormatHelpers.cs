using System.Text;
using Graphql.Mcp.DTO;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Helper methods for formatting output from tools
/// </summary>
public static class MarkdownFormatHelpers
{
    /// <summary>
    /// Formats a collection of items into a markdown section with a title and bulleted list
    /// </summary>
    /// <param name="title">The section title</param>
    /// <param name="items">The collection of items to format</param>
    /// <returns>Formatted markdown string or empty string if no items</returns>
    public static string FormatToolSection(string title, IEnumerable<DynamicToolInfo> items)
    {
        var itemsList = items.ToList();
        return itemsList.Any()
            ? $"### {title}{Environment.NewLine}" +
              string.Join(Environment.NewLine, itemsList.Select(i => $"- **{i.ToolName}**: {i.Description}")) +
              Environment.NewLine + Environment.NewLine
            : string.Empty;
    }

    /// <summary>
    /// Formats a collection of items into a bulleted list with optional formatting for each item
    /// </summary>
    /// <typeparam name="T">Type of items to format</typeparam>
    /// <param name="items">Collection of items</param>
    /// <param name="formatter">Function to format each item</param>
    /// <returns>Formatted markdown bulleted list</returns>
    public static string FormatBulletedList<T>(IEnumerable<T> items, Func<T, string> formatter)
    {
        var list = items.ToList();
        if (!list.Any())
            return string.Empty;

        return string.Join(Environment.NewLine, list.Select(item => $"- {formatter(item)}"));
    }

    /// <summary>
    /// Creates a markdown header with the specified level
    /// </summary>
    /// <param name="text">Header text</param>
    /// <param name="level">Header level (1-6)</param>
    /// <returns>Formatted markdown header</returns>
    public static string Header(string text, int level = 1)
    {
        level = Math.Clamp(level, 1, 6);
        return $"{new string('#', level)} {text}{Environment.NewLine}{Environment.NewLine}";
    }

    /// <summary>
    /// Formats key-value pairs as markdown metadata
    /// </summary>
    /// <param name="data">Dictionary of key-value pairs</param>
    /// <returns>Formatted markdown metadata</returns>
    public static string FormatMetadata(Dictionary<string, string> data)
    {
        var sb = new StringBuilder();
        foreach (var kvp in data)
        {
            sb.AppendLine($"**{kvp.Key}:** {kvp.Value}");
        }

        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>
    /// Formats a GraphQL query result with configuration details
    /// </summary>
    public static string FormatQueryResult(string query, string operationName, int maxDepth, bool includeAllScalars, string? variables)
    {
        var result = new StringBuilder();
        result.AppendLine("# Automatically Generated GraphQL Query\n");
        result.AppendLine("## Query");
        result.AppendLine("```graphql");
        result.AppendLine(query);
        result.AppendLine("```\n");

        result.AppendLine("## Configuration");
        result.AppendLine($"- **Operation:** {operationName}");
        result.AppendLine($"- **Max Depth:** {maxDepth}");
        result.AppendLine($"- **Include All Scalars:** {includeAllScalars}");

        if (!string.IsNullOrEmpty(variables))
        {
            result.AppendLine($"- **Variables:** {variables}");
        }

        return result.ToString();
    }

    /// <summary>
    /// Formats a nested field selection result for GraphQL types
    /// </summary>
    public static string FormatNestedSelectionResult(string selection, string typeName)
    {
        var result = new StringBuilder();
        result.AppendLine($"# Nested Field Selection for {typeName}\n");
        result.AppendLine("## Field Selection");
        result.AppendLine("```graphql");
        result.AppendLine("{");
        result.Append(selection);
        result.AppendLine("}");
        result.AppendLine("```");

        return result.ToString();
    }
}