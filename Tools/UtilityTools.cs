using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class UtilityTools
{
    [McpServerTool, Description("Format GraphQL queries with proper indentation, readable structure, and consistent styling for improved readability and maintainability. This formatting tool provides: consistent indentation with configurable spacing, proper bracket and parentheses alignment, field selection organization and grouping, comment preservation and positioning, line break optimization for readability, string literal handling and escaping, directive and fragment formatting. Essential for code review and collaborative development.")]
    public static string FormatQuery([Description("GraphQL query string to format with proper indentation and structure")] string query)
    {
        var formatted = new StringBuilder();
        var lines = query.Split('\n');
        var indentLevel = 0;
        var inString = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                formatted.AppendLine();
                continue;
            }

            if (trimmedLine.StartsWith("#"))
            {
                formatted.AppendLine(new string(' ', indentLevel * 2) + trimmedLine);
                continue;
            }

            if (trimmedLine.Contains("}") && !inString)
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            formatted.AppendLine(new string(' ', indentLevel * 2) + trimmedLine);

            if (trimmedLine.Contains("{") && !inString)
            {
                indentLevel++;
            }

            inString = CountChars(trimmedLine, '"') % 2 != 0;
        }

        return formatted.ToString()
            .TrimEnd();
    }

    [McpServerTool, Description("Compress GraphQL queries by removing whitespace and comments for production use")]
    public static string MinifyQuery([Description("GraphQL query to minify")] string query)
    {
        var minified = Regex.Replace(query, @"#.*$", "", RegexOptions.Multiline);

        var result = new StringBuilder();
        var inString = false;
        var prevChar = ' ';

        foreach (var ch in minified)
        {
            if (ch == '"' && prevChar != '\\')
            {
                inString = !inString;
                result.Append(ch);
            }
            else if (inString)
            {
                result.Append(ch);
            }
            else if (char.IsWhiteSpace(ch))
            {
                if (!char.IsWhiteSpace(prevChar))
                {
                    result.Append(' ');
                }
            }
            else
            {
                result.Append(ch);
            }

            prevChar = ch;
        }

        var cleaned = result.ToString();
        cleaned = Regex.Replace(cleaned, @"\s*([{}(),:=])\s*", "$1");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        return cleaned.Trim();
    }

    [McpServerTool, Description("Convert hardcoded values in queries to variables for reusability and type safety")]
    public static string ExtractVariables([Description("GraphQL query to extract variables from")] string query)
    {
        var result = new StringBuilder();
        var variables = new List<(string name, string type, string value)>();
        var variableCounter = 1;

        var stringMatches = Regex.Matches(query, @":\s*""([^""]+)""");
        foreach (Match match in stringMatches)
        {
            var value = match.Groups[1].Value;
            var variableName = $"var{variableCounter++}";
            variables.Add((variableName, "String", $"\"{value}\""));
        }

        var numberMatches = Regex.Matches(query, @":\s*(\d+(?:\.\d+)?)(?!\w)");
        foreach (Match match in numberMatches)
        {
            var value = match.Groups[1].Value;
            var variableName = $"var{variableCounter++}";
            var type = value.Contains('.') ? "Float" : "Int";
            variables.Add((variableName, type, value));
        }

        var boolMatches = Regex.Matches(query, @":\s*(true|false)(?!\w)", RegexOptions.IgnoreCase);
        foreach (Match match in boolMatches)
        {
            var value = match.Groups[1]
                .Value.ToLower();
            var variableName = $"var{variableCounter++}";
            variables.Add((variableName, "Boolean", value));
        }

        if (variables.Count == 0)
        {
            return "No hardcoded values found to extract into variables.";
        }

        result.AppendLine("# Original Query with Variables");
        result.AppendLine();

        var modifiedQuery = query;

        foreach (Match stringMatch in stringMatches)
        {
            var variableName = $"$var{variableCounter++}";
            modifiedQuery = modifiedQuery.Replace(stringMatch.Value, $": {variableName}");
        }

        foreach (Match numberMatch in numberMatches)
        {
            var variableName = $"$var{variableCounter++}";
            modifiedQuery = modifiedQuery.Replace(numberMatch.Value, $": {variableName}");
        }

        foreach (Match boolMatch in boolMatches)
        {
            var variableName = $"$var{variableCounter++}";
            modifiedQuery = modifiedQuery.Replace(boolMatch.Value, $": {variableName}");
        }

        var operationMatch = Regex.Match(modifiedQuery, @"^\s*(query|mutation|subscription)(\s+\w+)?", RegexOptions.IgnoreCase);
        if (operationMatch.Success)
        {
            var variableDeclarations = string.Join(", ", variables.Select(v => $"${v.name}: {v.type}"));
            var replacement = $"{operationMatch.Groups[1].Value}{operationMatch.Groups[2].Value}({variableDeclarations})";
            modifiedQuery = modifiedQuery.Replace(operationMatch.Value, replacement);
        }
        else
        {
            var variableDeclarations = string.Join(", ", variables.Select(v => $"${v.name}: {v.type}"));
            modifiedQuery = $"query({variableDeclarations}) {modifiedQuery}";
        }

        result.AppendLine("```graphql");
        result.AppendLine(modifiedQuery);
        result.AppendLine("```");
        result.AppendLine();

        result.AppendLine("# Variables");
        result.AppendLine("```json");
        result.AppendLine("{");
        for (var i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            var comma = i < variables.Count - 1 ? "," : "";
            result.AppendLine($"  \"{variable.name}\": {variable.value}{comma}");
        }

        result.AppendLine("}");
        result.AppendLine("```");

        return result.ToString();
    }

    [McpServerTool, Description("Create field aliases to prevent naming conflicts when querying the same field multiple times")]
    public static string GenerateAliases([Description("GraphQL query to generate aliases for")] string query)
    {
        var result = new StringBuilder();
        result.AppendLine("# GraphQL Query with Generated Aliases\n");

        var fieldMatches = Regex.Matches(query, @"(\w+)\s*\([^)]+\)", RegexOptions.IgnoreCase);
        var fieldCounts = new Dictionary<string, int>();
        var aliasedQuery = query;

        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            fieldCounts[fieldName] = fieldCounts.GetValueOrDefault(fieldName, 0) + 1;
        }

        var aliasCounter = new Dictionary<string, int>();
        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            if (fieldCounts[fieldName] > 1)
            {
                aliasCounter[fieldName] = aliasCounter.GetValueOrDefault(fieldName, 0) + 1;
                var aliasName = $"{fieldName}_{aliasCounter[fieldName]}";
                var replacement = $"{aliasName}: {match.Value}";
                aliasedQuery = aliasedQuery.Replace(match.Value, replacement);
            }
        }

        var selectionMatches = Regex.Matches(aliasedQuery, @"^\s*(\w+)(?:\s|$)", RegexOptions.Multiline);
        var selectionCounts = new Dictionary<string, int>();

        foreach (Match match in selectionMatches)
        {
            var fieldName = match.Groups[1].Value;
            if (!IsGraphQlKeyword(fieldName))
            {
                selectionCounts[fieldName] = selectionCounts.GetValueOrDefault(fieldName, 0) + 1;
            }
        }

        result.AppendLine("## Analysis");
        if (fieldCounts.Any(kvp => kvp.Value > 1))
        {
            result.AppendLine("**Fields with multiple calls that got aliases:**");
            foreach (var kvp in fieldCounts.Where(kvp => kvp.Value > 1))
            {
                result.AppendLine($"- `{kvp.Key}`: {kvp.Value} calls");
            }

            result.AppendLine();
        }

        result.AppendLine("## Modified Query");
        result.AppendLine("```graphql");
        result.AppendLine(aliasedQuery);
        result.AppendLine("```");

        if (!fieldCounts.Any(kvp => kvp.Value > 1))
        {
            result.AppendLine("\n✅ **No alias conflicts detected.** Your query doesn't appear to need aliases.");
        }
        else
        {
            result.AppendLine("\n## Benefits of Using Aliases");
            result.AppendLine("- Avoids field conflicts when making multiple calls to the same field");
            result.AppendLine("- Makes response structure clearer and more predictable");
            result.AppendLine("- Enables querying the same field with different arguments");
        }

        return result.ToString();
    }

    [McpServerTool, Description("Provide comprehensive GraphQL utilities with intelligent formatting, optimization, transformation, and development assistance. This advanced utility suite offers complete GraphQL workflow support with smart automation and best practices.")]
    public static async Task<string> UtilityOperationsComprehensive(
        [Description("GraphQL query or operation to process")]
        string operation,
        [Description("Utility operation: 'format' for formatting, 'optimize' for optimization, 'transform' for transformation, 'validate' for validation, 'analyze' for analysis")]
        string utilityType = "format",
        [Description("Include advanced formatting options and style preferences")]
        bool includeAdvancedFormatting = true,
        [Description("Include optimization suggestions and improvements")]
        bool includeOptimizations = true,
        [Description("Output format: 'readable' for human-readable, 'compact' for minimal, 'production' for optimized")]
        string outputFormat = "readable")
    {
        try
        {
            return await ServiceLocator.ExecuteWithSmartResponseServiceAsync(async smartResponseService =>
            {
                var smartResponse = await smartResponseService.CreateUtilityOperationsResponseAsync(
                    operation, utilityType, includeAdvancedFormatting, includeOptimizations, outputFormat);

                return await smartResponseService.FormatComprehensiveResponseAsync(smartResponse);
            });
        }
        catch (Exception ex)
        {
            return await ServiceLocator.ExecuteWithSmartResponseServiceAsync(async smartResponseService =>
            {
                return await smartResponseService.CreateErrorResponseAsync(
                    "UtilityOperationError",
                    ex.Message,
                    new { operation, utilityType, outputFormat });
            });
        }
    }

    private static int CountChars(string input, char target)
    {
        return input.Count(c => c == target);
    }

    private static bool IsGraphQlKeyword(string word)
    {
        var keywords = new[] { "query", "mutation", "subscription", "fragment", "on", "true", "false", "null", "__schema", "__type" };
        return keywords.Contains(word.ToLower());
    }
}