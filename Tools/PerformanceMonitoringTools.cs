using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class PerformanceMonitoringTools
{
    [McpServerTool, Description("Measure GraphQL query execution time and generate performance reports")]
    public static async Task<string> MeasureQueryPerformance(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("GraphQL query to measure")]
        string query,
        [Description("Number of test runs")] int runs = 5,
        [Description("Variables as JSON object (optional)")]
        string? variables = null,
        [Description("HTTP headers as JSON object (optional)")]
        string? headers = null)
    {
        try
        {
            var measurements = new List<TimeSpan>();
            var results = new StringBuilder();
            results.AppendLine("# GraphQL Query Performance Report\n");

            var requestBody = new
            {
                query,
                variables = string.IsNullOrWhiteSpace(variables) ? null : JsonSerializer.Deserialize<object>(variables)
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);

            results.AppendLine("## Test Configuration");
            results.AppendLine($"- **Endpoint:** {endpoint}");
            results.AppendLine($"- **Runs:** {runs}");
            results.AppendLine($"- **Query Length:** {query.Length} characters");
            results.AppendLine($"- **Has Variables:** {!string.IsNullOrWhiteSpace(variables)}");
            results.AppendLine();

            // Warm up run
            results.AppendLine("## Executing Performance Tests...\n");
            try
            {
                await HttpClientHelper.ExecuteGraphQlRequestAsync(endpoint, requestBody, headers);
                results.AppendLine("✅ Warmup run completed");
            }
            catch
            {
                results.AppendLine("⚠️ Warmup run failed, continuing with tests");
            }

            // Performance test runs
            for (var i = 0; i < runs; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpoint, requestBody, headers);
                    stopwatch.Stop();

                    if (result.IsSuccess)
                    {
                        measurements.Add(stopwatch.Elapsed);
                        results.AppendLine($"Run {i + 1}: {stopwatch.Elapsed.TotalMilliseconds:F2}ms ✅");
                    }
                    else
                    {
                        results.AppendLine($"Run {i + 1}: Failed - {result.ErrorMessage} ❌");
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    results.AppendLine($"Run {i + 1}: Error - {ex.Message} ❌");
                }
            }

            if (measurements.Count == 0)
            {
                results.AppendLine("\n❌ **No successful measurements obtained.**");
                return results.ToString();
            }

            // Calculate statistics
            var avgMs = measurements.Average(m => m.TotalMilliseconds);
            var minMs = measurements.Min(m => m.TotalMilliseconds);
            var maxMs = measurements.Max(m => m.TotalMilliseconds);
            var medianMs = CalculateMedian(measurements.Select(m => m.TotalMilliseconds)
                .ToList());

            results.AppendLine("\n## Performance Statistics");
            results.AppendLine($"- **Average:** {avgMs:F2}ms");
            results.AppendLine($"- **Median:** {medianMs:F2}ms");
            results.AppendLine($"- **Min:** {minMs:F2}ms");
            results.AppendLine($"- **Max:** {maxMs:F2}ms");
            results.AppendLine($"- **Range:** {(maxMs - minMs):F2}ms");
            results.AppendLine($"- **Successful Runs:** {measurements.Count}/{runs}");

            // Performance assessment
            results.AppendLine("\n## Performance Assessment");
            if (avgMs < 100)
            {
                results.AppendLine("🟢 **Excellent Performance** - Query executes very quickly");
            }
            else if (avgMs < 500)
            {
                results.AppendLine("🟡 **Good Performance** - Query executes reasonably fast");
            }
            else if (avgMs < 1000)
            {
                results.AppendLine("🟠 **Moderate Performance** - Query execution time is noticeable");
            }
            else
            {
                results.AppendLine("🔴 **Poor Performance** - Query takes significant time to execute");
            }

            // Recommendations
            results.AppendLine("\n## Recommendations");
            if (avgMs > 500)
            {
                results.AppendLine("- Consider optimizing field selections");
                results.AppendLine("- Review query complexity and nesting levels");
                results.AppendLine("- Check if query can be broken into smaller parts");
            }

            if (maxMs - minMs > avgMs)
            {
                results.AppendLine("- High variance detected - consider server load balancing");
                results.AppendLine("- Network conditions may be affecting performance");
            }

            return results.ToString();
        }
        catch (Exception ex)
        {
            return $"Error measuring query performance: {ex.Message}";
        }
    }

    [McpServerTool, Description("Identify potential N+1 query problems and recommend DataLoader optimization patterns")]
    public static string AnalyzeDataLoaderPatterns([Description("GraphQL query to analyze")] string query)
    {
        try
        {
            var analysis = new StringBuilder();
            analysis.AppendLine("# DataLoader Pattern Analysis\n");

            // Analyze the query structure for potential N+1 problems
            var nestingAnalysis = AnalyzeNestingPatterns(query);
            var fieldAnalysis = AnalyzeFieldPatterns(query);
            var listFieldAnalysis = AnalyzeListFields(query);

            analysis.AppendLine("## Query Structure Analysis");
            analysis.AppendLine($"- **Maximum Nesting Level:** {nestingAnalysis.MaxNesting}");
            analysis.AppendLine($"- **Total Field Selections:** {fieldAnalysis.TotalFields}");
            analysis.AppendLine($"- **Unique Fields:** {fieldAnalysis.UniqueFields}");
            analysis.AppendLine($"- **Potentially List Fields:** {listFieldAnalysis.Count}");
            analysis.AppendLine();

            // Detect potential N+1 patterns
            var potentialN1Issues = DetectPotentialN1Issues(query);

            if (potentialN1Issues.Count > 0)
            {
                analysis.AppendLine("## ⚠️ Potential N+1 Issues Detected");
                foreach (var issue in potentialN1Issues)
                {
                    analysis.AppendLine($"- **{issue.FieldPath}**: {issue.Description}");
                }

                analysis.AppendLine();

                analysis.AppendLine("## 🔧 DataLoader Recommendations");
                foreach (var issue in potentialN1Issues)
                {
                    analysis.AppendLine($"### {issue.FieldPath}");
                    analysis.AppendLine(GenerateDataLoaderRecommendation(issue));
                    analysis.AppendLine();
                }
            }
            else
            {
                analysis.AppendLine("## ✅ No Obvious N+1 Issues Detected");
                analysis.AppendLine("Your query structure looks good from a DataLoader perspective!");
                analysis.AppendLine();
            }

            // General DataLoader best practices
            analysis.AppendLine("## 💡 General DataLoader Best Practices");
            analysis.AppendLine("1. **Batch Related Queries**: Group database queries for the same resource type");
            analysis.AppendLine("2. **Cache Results**: Use DataLoader's built-in caching for the request lifecycle");
            analysis.AppendLine("3. **Avoid Over-fetching**: Only load fields that are actually requested");
            analysis.AppendLine("4. **Consider Depth Limiting**: Implement query depth limits for deeply nested queries");
            analysis.AppendLine("5. **Monitor Performance**: Track query performance and database query counts");

            // Example DataLoader implementation
            if (potentialN1Issues.Count > 0)
            {
                analysis.AppendLine("\n## 📝 Example DataLoader Implementation");
                analysis.AppendLine("```csharp");
                analysis.AppendLine("public class UserDataLoader : DataLoaderBase<int, User>");
                analysis.AppendLine("{");
                analysis.AppendLine("    private readonly IUserRepository _userRepository;");
                analysis.AppendLine();
                analysis.AppendLine("    public UserDataLoader(IUserRepository userRepository)");
                analysis.AppendLine("    {");
                analysis.AppendLine("        _userRepository = userRepository;");
                analysis.AppendLine("    }");
                analysis.AppendLine();
                analysis.AppendLine("    protected override async Task<IDictionary<int, User>> FetchAsync(");
                analysis.AppendLine("        IEnumerable<int> keys, CancellationToken cancellationToken)");
                analysis.AppendLine("    {");
                analysis.AppendLine("        var users = await _userRepository.GetByIdsAsync(keys.ToList());");
                analysis.AppendLine("        return users.ToDictionary(u => u.Id);");
                analysis.AppendLine("    }");
                analysis.AppendLine("}");
                analysis.AppendLine("```");
            }

            return analysis.ToString();
        }
        catch (Exception ex)
        {
            return $"Error analyzing DataLoader patterns: {ex.Message}";
        }
    }

    private static double CalculateMedian(List<double> values)
    {
        values.Sort();
        var count = values.Count;

        if (count % 2 == 0)
        {
            return (values[count / 2 - 1] + values[count / 2]) / 2.0;
        }
        else
        {
            return values[count / 2];
        }
    }

    private static (int MaxNesting, List<string> NestedPaths) AnalyzeNestingPatterns(string query)
    {
        var maxNesting = 0;
        var currentNesting = 0;
        var nestedPaths = new List<string>();
        var currentPath = new List<string>();

        var fieldPattern = @"\b(\w+)\s*(?:\([^)]*\))?\s*\{";
        var matches = Regex.Matches(query, fieldPattern);

        foreach (Match match in matches)
        {
            var fieldName = match.Groups[1].Value;
            if (!IsGraphQlKeyword(fieldName))
            {
                currentPath.Add(fieldName);
                currentNesting++;
                maxNesting = Math.Max(maxNesting, currentNesting);

                if (currentNesting > 2)
                {
                    nestedPaths.Add(string.Join(".", currentPath));
                }
            }
        }

        return (maxNesting, nestedPaths);
    }

    private static (int TotalFields, int UniqueFields) AnalyzeFieldPatterns(string query)
    {
        var fieldMatches = Regex.Matches(query, @"\b(\w+)(?:\s*\([^)]*\))?\s*(?:\{|$)", RegexOptions.IgnoreCase);
        var fields = new List<string>();

        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            if (!IsGraphQlKeyword(fieldName))
            {
                fields.Add(fieldName);
            }
        }

        return (fields.Count, fields.Distinct()
            .Count());
    }

    private static List<string> AnalyzeListFields(string query)
    {
        var potentialListFields = new List<string>();

        // Look for fields that are likely to return lists (common naming patterns)
        var listPatterns = new[] { "s$", "list$", "items$", "collection$", "all$" };
        var fieldMatches = Regex.Matches(query, @"\b(\w+)(?:\s*\([^)]*\))?\s*\{", RegexOptions.IgnoreCase);

        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            if (listPatterns.Any(pattern => Regex.IsMatch(fieldName, pattern, RegexOptions.IgnoreCase)))
            {
                potentialListFields.Add(fieldName);
            }
        }

        return potentialListFields;
    }

    private static List<N1Issue> DetectPotentialN1Issues(string query)
    {
        var issues = new List<N1Issue>();

        // Pattern 1: List field followed by scalar field selections (classic N+1)
        var listFieldPattern = @"(\w+s|\w+List|\w+Collection)\s*(?:\([^)]*\))?\s*\{([^}]+)\}";
        var listMatches = Regex.Matches(query, listFieldPattern, RegexOptions.IgnoreCase);

        foreach (Match match in listMatches)
        {
            var listField = match.Groups[1].Value;
            var innerFields = match.Groups[2].Value;

            // Check if inner fields contain object relationships
            var objectFieldMatches = Regex.Matches(innerFields, @"\b(\w+)\s*\{");
            if (objectFieldMatches.Count > 0)
            {
                foreach (Match objMatch in objectFieldMatches)
                {
                    var objectField = objMatch.Groups[1].Value;
                    issues.Add(new N1Issue
                    {
                        FieldPath = $"{listField}.{objectField}",
                        Description = $"List field '{listField}' contains object field '{objectField}' which may cause N+1 queries",
                        Severity = "High",
                        Type = "ListWithObjectFields"
                    });
                }
            }
        }

        // Pattern 2: Deeply nested selections
        var nestingLevel = 0;
        var path = new List<string>();
        var fieldPattern = @"\b(\w+)\s*(?:\([^)]*\))?\s*\{";
        var fieldMatches = Regex.Matches(query, fieldPattern);

        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            if (!IsGraphQlKeyword(fieldName))
            {
                path.Add(fieldName);
                nestingLevel++;

                if (nestingLevel > 3)
                {
                    issues.Add(new N1Issue
                    {
                        FieldPath = string.Join(".", path),
                        Description = "Deep nesting may indicate potential for N+1 queries",
                        Severity = "Medium",
                        Type = "DeepNesting"
                    });
                }
            }
        }

        return issues;
    }

    private static string GenerateDataLoaderRecommendation(N1Issue issue)
    {
        return issue.Type switch
        {
            "ListWithObjectFields" =>
                $"Consider implementing a DataLoader for the '{issue.FieldPath.Split('.').Last()}' relationship. " +
                "This will batch the database queries instead of making individual queries for each item in the list.",

            "DeepNesting" =>
                "Consider implementing DataLoaders at each level of nesting to batch queries. " +
                "Also consider if this level of nesting is necessary or if the query can be restructured.",

            _ => "Consider using DataLoader patterns to batch and cache database queries."
        };
    }

    private static bool IsGraphQlKeyword(string word)
    {
        var keywords = new[] { "query", "mutation", "subscription", "fragment", "on", "true", "false", "null" };
        return keywords.Contains(word.ToLower());
    }

    private class N1Issue
    {
        public string FieldPath { get; set; } = "";
        public string Description { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Type { get; set; } = "";
    }
}