using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class UtilityTools
{
    [McpServerTool, Description("Format and prettify GraphQL queries")]
    public static string FormatQuery([Description("GraphQL query to format")] string query)
    {
        var formatted = new StringBuilder();
        var lines = query.Split('\n');
        var indentLevel = 0;
        var inString = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                formatted.AppendLine();
                continue;
            }

            // Handle comments
            if (trimmedLine.StartsWith("#"))
            {
                formatted.AppendLine(new string(' ', indentLevel * 2) + trimmedLine);
                continue;
            }

            // Adjust indentation for closing braces
            if (trimmedLine.Contains("}") && !inString)
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            // Add the line with proper indentation
            formatted.AppendLine(new string(' ', indentLevel * 2) + trimmedLine);

            // Adjust indentation for opening braces
            if (trimmedLine.Contains("{") && !inString)
            {
                indentLevel++;
            }

            // Track string literals (simplified)
            inString = CountChars(trimmedLine, '"') % 2 != 0;
        }

        return formatted.ToString()
            .TrimEnd();
    }

    [McpServerTool, Description("Minify GraphQL queries for production use")]
    public static string MinifyQuery([Description("GraphQL query to minify")] string query)
    {
        // Remove comments
        var minified = Regex.Replace(query, @"#.*$", "", RegexOptions.Multiline);

        // Remove unnecessary whitespace while preserving string literals
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
                // Only add one space if the previous character wasn't already whitespace
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

        // Clean up extra spaces around punctuation
        var cleaned = result.ToString();
        cleaned = Regex.Replace(cleaned, @"\s*([{}(),:=])\s*", "$1");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        return cleaned.Trim();
    }

    [McpServerTool, Description("Extract hardcoded values into variables")]
    public static string ExtractVariables([Description("GraphQL query to extract variables from")] string query)
    {
        var result = new StringBuilder();
        var variables = new List<(string name, string type, string value)>();
        var variableCounter = 1;

        // Find string literals
        var stringMatches = Regex.Matches(query, @":\s*""([^""]+)""");
        foreach (Match match in stringMatches)
        {
            var value = match.Groups[1].Value;
            var variableName = $"var{variableCounter++}";
            variables.Add((variableName, "String", $"\"{value}\""));
        }

        // Find number literals
        var numberMatches = Regex.Matches(query, @":\s*(\d+(?:\.\d+)?)(?!\w)");
        foreach (Match match in numberMatches)
        {
            var value = match.Groups[1].Value;
            var variableName = $"var{variableCounter++}";
            var type = value.Contains('.') ? "Float" : "Int";
            variables.Add((variableName, type, value));
        }

        // Find boolean literals
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

        // Generate the query with variables
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

        // Add variable declarations to query
        var operationMatch = Regex.Match(modifiedQuery, @"^\s*(query|mutation|subscription)(\s+\w+)?", RegexOptions.IgnoreCase);
        if (operationMatch.Success)
        {
            var variableDeclarations = string.Join(", ", variables.Select(v => $"${v.name}: {v.type}"));
            var replacement = $"{operationMatch.Groups[1].Value}{operationMatch.Groups[2].Value}({variableDeclarations})";
            modifiedQuery = modifiedQuery.Replace(operationMatch.Value, replacement);
        }
        else
        {
            // Anonymous query - need to add query wrapper
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

    [McpServerTool, Description("Generate field aliases to avoid conflicts in complex queries")]
    public static string GenerateAliases([Description("GraphQL query to generate aliases for")] string query)
    {
        var result = new StringBuilder();
        result.AppendLine("# GraphQL Query with Generated Aliases\n");

        // Find field calls with arguments that might need aliases
        var fieldMatches = Regex.Matches(query, @"(\w+)\s*\([^)]+\)", RegexOptions.IgnoreCase);
        var fieldCounts = new Dictionary<string, int>();
        var aliasedQuery = query;

        // Count field occurrences
        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            fieldCounts[fieldName] = fieldCounts.GetValueOrDefault(fieldName, 0) + 1;
        }

        // Generate aliases for fields that appear multiple times
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

        // Also look for potential conflicts in selections without arguments
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