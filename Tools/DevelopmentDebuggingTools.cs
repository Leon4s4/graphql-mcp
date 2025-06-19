using System.ComponentModel;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class DevelopmentDebuggingTools
{
    [McpServerTool, Description("Provide detailed human-readable analysis of what a GraphQL query does, including operation breakdown, field selections, data flow patterns, and complexity assessment. This educational tool helps understand: operation type identification and structure, variable declarations and usage patterns, field selection hierarchy and relationships, fragment definitions and usage, directive applications and behavior, query complexity and performance implications, data flow visualization and dependencies. Perfect for learning GraphQL or debugging complex queries.")]
    public static string ExplainQuery([Description("GraphQL query string to analyze and explain in detail")] string query)
    {
        var explanation = new List<string>();
        explanation.Add("# GraphQL Query Analysis\n");

        // Detect operation type
        var operationMatch = Regex.Match(query, @"^\s*(query|mutation|subscription)\s+(\w+)?", RegexOptions.IgnoreCase);
        if (operationMatch.Success)
        {
            var operationType = operationMatch.Groups[1]
                .Value.ToLower();
            var operationName = operationMatch.Groups[2].Success ? operationMatch.Groups[2].Value : "anonymous";
            explanation.Add($"**Operation Type:** {operationType}");
            explanation.Add($"**Operation Name:** {operationName}");
        }
        else if (query.TrimStart()
                 .StartsWith("{"))
        {
            explanation.Add("**Operation Type:** query (anonymous)");
        }

        // Extract variables
        var variableMatches = Regex.Matches(query, @"\$(\w+):\s*([^,\)]+)", RegexOptions.IgnoreCase);
        if (variableMatches.Count > 0)
        {
            explanation.Add("\n**Variables:**");
            foreach (Match match in variableMatches)
            {
                explanation.Add($"- `${match.Groups[1].Value}`: {match.Groups[2].Value.Trim()}");
            }
        }

        // Extract field selections
        explanation.Add("\n**Field Selections:**");
        var fieldSelections = ExtractFieldSelections(query);
        foreach (var selection in fieldSelections)
        {
            explanation.Add($"- {selection}");
        }

        // Extract fragments
        var fragmentMatches = Regex.Matches(query, @"fragment\s+(\w+)\s+on\s+(\w+)", RegexOptions.IgnoreCase);
        if (fragmentMatches.Count > 0)
        {
            explanation.Add("\n**Fragments:**");
            foreach (Match match in fragmentMatches)
            {
                explanation.Add($"- `{match.Groups[1].Value}` on type `{match.Groups[2].Value}`");
            }
        }

        // Extract directives
        var directiveMatches = Regex.Matches(query, @"@(\w+)(?:\([^)]*\))?", RegexOptions.IgnoreCase);
        if (directiveMatches.Count > 0)
        {
            explanation.Add("\n**Directives Used:**");
            var uniqueDirectives = directiveMatches.Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .Distinct();
            foreach (var directive in uniqueDirectives)
            {
                explanation.Add($"- `@{directive}`");
            }
        }

        // Analyze complexity
        var fieldCount = Regex.Matches(query, @"\w+(?=\s*[{(]|\s*$)", RegexOptions.IgnoreCase)
            .Count;
        explanation.Add($"\n**Estimated Complexity:**");
        explanation.Add($"- Field count: {fieldCount}");

        var nestingLevel = AnalyzeNestingLevel(query);
        explanation.Add($"- Max nesting level: {nestingLevel}");

        return string.Join("\n", explanation);
    }

    [McpServerTool, Description("Suggest performance optimizations and best practice improvements for GraphQL queries with actionable recommendations. This optimization tool provides suggestions for: reducing over-fetching and under-fetching, fragment usage for code reuse, query depth and complexity reduction, field selection optimization, variable usage improvements, directive application enhancements, caching strategy recommendations, pagination and filtering optimizations. Essential for production query optimization and performance tuning.")]
    public static string OptimizeQuery([Description("GraphQL query string to analyze for optimization opportunities")] string query)
    {
        var suggestions = new List<string>();
        suggestions.Add("# GraphQL Query Optimization Suggestions\n");

        // Check for potential over-fetching
        var fieldSelections = ExtractFieldSelections(query);
        var uniqueFields = fieldSelections.Distinct()
            .ToList();

        if (fieldSelections.Count != uniqueFields.Count)
        {
            suggestions.Add("⚠️ **Duplicate Field Selections Detected**");
            suggestions.Add("Consider removing duplicate field selections to reduce query size.");
        }

        // Check for missing fragments
        var repeatedSelections = fieldSelections.GroupBy(f => f)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        if (repeatedSelections.Any())
        {
            suggestions.Add("\n💡 **Fragment Extraction Opportunity**");
            suggestions.Add("Consider extracting repeated field selections into fragments:");
            foreach (var repeated in repeatedSelections)
            {
                suggestions.Add($"- `{repeated}` (appears {fieldSelections.Count(f => f == repeated)} times)");
            }
        }

        // Check nesting depth
        var nestingLevel = AnalyzeNestingLevel(query);
        if (nestingLevel > 5)
        {
            suggestions.Add($"\n⚠️ **Deep Nesting Warning** (Level: {nestingLevel})");
            suggestions.Add("Consider breaking down the query or using pagination for deeply nested data.");
        }

        // Check for missing aliases on multiple calls to same field
        var aliasMatches = Regex.Matches(query, @"(\w+):\s*(\w+)", RegexOptions.IgnoreCase);
        if (aliasMatches.Count == 0)
        {
            var fieldMatches = Regex.Matches(query, @"\b(\w+)\s*[({]", RegexOptions.IgnoreCase);
            var fields = fieldMatches.Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();
            var duplicateFields = fields.GroupBy(f => f)
                .Where(g => g.Count() > 1);

            if (duplicateFields.Any())
            {
                suggestions.Add("\n💡 **Alias Suggestion**");
                suggestions.Add("Consider using aliases for multiple calls to the same field:");
                foreach (var duplicate in duplicateFields)
                {
                    suggestions.Add($"- `{duplicate.Key}` appears {duplicate.Count()} times");
                }
            }
        }

        // Suggest using variables for hardcoded values
        var hardcodedMatches = Regex.Matches(query, @":\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (hardcodedMatches.Count > 0)
        {
            suggestions.Add("\n💡 **Variable Extraction Suggestion**");
            suggestions.Add("Consider extracting hardcoded values into variables for reusability:");
            foreach (Match match in hardcodedMatches)
            {
                suggestions.Add($"- `\"{match.Groups[1].Value}\"`");
            }
        }

        if (suggestions.Count == 1) // Only header
        {
            suggestions.Add("✅ **No obvious optimization opportunities found.**");
            suggestions.Add("Your query appears to be well-structured!");
        }

        return string.Join("\n", suggestions);
    }

    [McpServerTool, Description("Identify repeated field patterns and generate reusable GraphQL fragments")]
    public static string ExtractFragments([Description("GraphQL query to extract fragments from")] string query)
    {
        var result = new List<string>();
        result.Add("# Fragment Extraction Results\n");

        // Find repeated field patterns
        var fieldPatterns = new Dictionary<string, List<string>>();
        var lines = query.Split('\n');
        var currentContext = "";
        var currentFields = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.Contains("{"))
            {
                // Starting a new selection set
                if (currentFields.Count > 0 && !string.IsNullOrEmpty(currentContext))
                {
                    var key = string.Join(", ", currentFields.OrderBy(f => f));
                    if (!fieldPatterns.ContainsKey(key))
                        fieldPatterns[key] = [];
                    fieldPatterns[key]
                        .Add(currentContext);
                }

                currentContext = trimmedLine.Replace("{", "")
                    .Trim();
                currentFields.Clear();
            }
            else if (trimmedLine.Contains("}"))
            {
                if (currentFields.Count > 0 && !string.IsNullOrEmpty(currentContext))
                {
                    var key = string.Join(", ", currentFields.OrderBy(f => f));
                    if (!fieldPatterns.ContainsKey(key))
                        fieldPatterns[key] = [];
                    fieldPatterns[key]
                        .Add(currentContext);
                }

                currentContext = "";
                currentFields.Clear();
            }
            else if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith("#"))
            {
                // Field selection
                var fieldName = trimmedLine.Split(' ')[0]
                    .Split('(')[0];
                currentFields.Add(fieldName);
            }
        }

        // Find patterns that repeat
        var candidatesForFragments = fieldPatterns
            .Where(kvp => kvp.Value.Count > 1 && kvp.Value.First()
                .Split(',')
                .Length > 2)
            .ToList();

        if (candidatesForFragments.Any())
        {
            result.Add("## Suggested Fragments:\n");

            var fragmentCounter = 1;
            foreach (var candidate in candidatesForFragments)
            {
                var fields = candidate.Key.Split(',')
                    .Select(f => f.Trim())
                    .ToList();
                var contexts = candidate.Value;

                result.Add($"### Fragment {fragmentCounter}: Common Fields");
                result.Add("```graphql");
                result.Add($"fragment CommonFields{fragmentCounter} on [TypeName] {{");
                foreach (var field in fields)
                {
                    result.Add($"  {field}");
                }

                result.Add("}");
                result.Add("```");

                result.Add($"**Used in contexts:** {string.Join(", ", contexts)}");
                result.Add($"**Potential savings:** {fields.Count * (contexts.Count - 1)} field repetitions\n");

                fragmentCounter++;
            }

            result.Add("## Usage Example:");
            result.Add("Replace repeated field selections with fragment spreads:");
            result.Add("```graphql");
            result.Add("query MyQuery {");
            result.Add("  user {");
            result.Add("    ...CommonFields1");
            result.Add("  }");
            result.Add("  profile {");
            result.Add("    ...CommonFields1");
            result.Add("  }");
            result.Add("}");
            result.Add("```");
        }
        else
        {
            result.Add("✅ **No obvious fragment extraction opportunities found.**");
            result.Add("Your query may already be well-optimized or may not have sufficient repetition to warrant fragments.");
        }

        return string.Join("\n", result);
    }

    [McpServerTool, Description("Calculate query complexity metrics including depth, field count, and performance score")]
    public static string AnalyzeQueryComplexity([Description("GraphQL query to analyze")] string query)
    {
        var analysis = new List<string>();
        analysis.Add("# GraphQL Query Complexity Analysis\n");

        // Basic metrics
        var fieldCount = CountFields(query);
        var nestingLevel = AnalyzeNestingLevel(query);
        var argumentCount = Regex.Matches(query, @"\([^)]*\)")
            .Count;
        var fragmentCount = Regex.Matches(query, @"\.\.\.(\w+)")
            .Count;
        var directiveCount = Regex.Matches(query, @"@\w+")
            .Count;

        // Calculate complexity score
        var complexityScore = CalculateComplexityScore(fieldCount, nestingLevel, argumentCount, fragmentCount, directiveCount);

        analysis.Add("## Complexity Metrics");
        analysis.Add($"- **Field Count:** {fieldCount}");
        analysis.Add($"- **Maximum Nesting Level:** {nestingLevel}");
        analysis.Add($"- **Arguments Used:** {argumentCount}");
        analysis.Add($"- **Fragments Used:** {fragmentCount}");
        analysis.Add($"- **Directives Used:** {directiveCount}");
        analysis.Add($"- **Overall Complexity Score:** {complexityScore}");

        // Provide assessment
        analysis.Add("\n## Complexity Assessment");
        if (complexityScore <= 10)
        {
            analysis.Add("✅ **Low Complexity** - This query should execute efficiently.");
        }
        else if (complexityScore <= 25)
        {
            analysis.Add("⚠️ **Medium Complexity** - Consider optimization if performance becomes an issue.");
        }
        else if (complexityScore <= 50)
        {
            analysis.Add("🔴 **High Complexity** - This query may be expensive to execute. Consider optimization.");
        }
        else
        {
            analysis.Add("🚨 **Very High Complexity** - This query is likely to cause performance issues. Optimization recommended.");
        }

        // Provide recommendations
        analysis.Add("\n## Recommendations");
        if (nestingLevel > 5)
        {
            analysis.Add("- Consider reducing nesting depth through pagination or query restructuring");
        }

        if (fieldCount > 20)
        {
            analysis.Add("- Consider breaking the query into smaller, focused queries");
        }

        if (argumentCount > 10)
        {
            analysis.Add("- Review if all arguments are necessary");
        }

        return string.Join("\n", analysis);
    }

    [McpServerTool, Description("Perform comprehensive development debugging with intelligent query analysis, performance diagnostics, schema validation, error investigation, and development workflow optimization. This advanced debugging tool provides complete development support.")]
    public static async Task<string> DebugDevelopmentIssuesComprehensive(
        [Description("GraphQL query string to debug and analyze")]
        string query,
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Debug focus: 'performance' for speed issues, 'errors' for error investigation, 'schema' for schema problems, 'workflow' for development process")]
        string debugFocus = "comprehensive",
        [Description("Include interactive debugging session with step-by-step analysis")]
        bool includeInteractiveDebugging = true,
        [Description("Include performance profiling and bottleneck detection")]
        bool includePerformanceProfiling = true,
        [Description("Error context for debugging specific issues")]
        string? errorContext = null)
    {
        try
        {
            var smartResponse = await SmartResponseService.Instance.CreateDevelopmentDebuggingResponseAsync(
                query, endpointName, debugFocus, includeInteractiveDebugging, includePerformanceProfiling, errorContext);
            
            return await SmartResponseService.Instance.FormatComprehensiveResponseAsync(smartResponse);
        }
        catch (Exception ex)
        {
            return await SmartResponseService.Instance.CreateErrorResponseAsync(
                "DevelopmentDebuggingError", 
                ex.Message,
                new { query, endpointName, debugFocus });
        }
    }

    private static List<string> ExtractFieldSelections(string query)
    {
        var fields = new List<string>();
        var matches = Regex.Matches(query, @"\b(\w+)(?:\s*\([^)]*\))?\s*(?:\{|$)", RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var fieldName = match.Groups[1].Value;
            if (!IsGraphQlKeyword(fieldName))
            {
                fields.Add(fieldName);
            }
        }

        return fields;
    }

    private static bool IsGraphQlKeyword(string word)
    {
        var keywords = new[] { "query", "mutation", "subscription", "fragment", "on", "true", "false", "null" };
        return keywords.Contains(word.ToLower());
    }

    private static int AnalyzeNestingLevel(string query)
    {
        var maxLevel = 0;
        var currentLevel = 0;

        foreach (var c in query)
        {
            if (c == '{')
            {
                currentLevel++;
                maxLevel = Math.Max(maxLevel, currentLevel);
            }
            else if (c == '}')
            {
                currentLevel--;
            }
        }

        return maxLevel;
    }

    private static int CountFields(string query)
    {
        var fieldMatches = Regex.Matches(query, @"\b[a-zA-Z_][a-zA-Z0-9_]*\b(?=\s*[{(:]|\s*$)");
        return fieldMatches.Count;
    }

    private static int CalculateComplexityScore(int fieldCount, int nestingLevel, int argumentCount, int fragmentCount, int directiveCount)
    {
        // Weighted complexity calculation
        return (fieldCount * 1) +
               (nestingLevel * 3) +
               (argumentCount * 2) +
               (fragmentCount * 1) +
               (directiveCount * 1);
    }
}