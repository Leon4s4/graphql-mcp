using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class FieldUsageAnalyticsTools
{
    [McpServerTool, Description("Analyze GraphQL field usage patterns from query logs to identify performance optimization opportunities, unused schema fields, and popular API patterns. This tool provides insights including: field popularity rankings and usage statistics, unused field identification for schema cleanup, deprecation candidates based on low usage, query pattern analysis and trends, performance impact assessment by field complexity, schema evolution recommendations. Essential for API governance and optimization.")]
    public static async Task<string> AnalyzeFieldUsage(
        [Description("Log of executed queries as JSON array. Example: [\"query { users { id name } }\", \"query { posts { title author { name } } }\"]")]
        string queryLog,
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Include unused fields in the analysis report. Useful for schema cleanup")]
        bool showUnused = true)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var result = new StringBuilder();
        result.AppendLine("# Field Usage Analytics Report\n");

        var queries = JsonSerializer.Deserialize<string[]>(queryLog);
        if (queries == null || queries.Length == 0)
        {
            return "Error: No queries found in log";
        }

        var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
            return schemaResult.FormatForDisplay();

        var schemaFields = await ExtractSchemaFields(schemaResult.Content!);

        var usageStats = AnalyzeFieldUsageFromQueries(queries, schemaFields);

        result.AppendLine("## Usage Summary");
        result.AppendLine($"- **Total Queries Analyzed:** {queries.Length}");
        result.AppendLine($"- **Total Schema Fields:** {schemaFields.Count}");
        result.AppendLine($"- **Used Fields:** {usageStats.Count(s => s.UsageCount > 0)}");
        result.AppendLine($"- **Unused Fields:** {usageStats.Count(s => s.UsageCount == 0)}");
        result.AppendLine($"- **Usage Rate:** {(usageStats.Count(s => s.UsageCount > 0) / (double)schemaFields.Count):P1}\n");

        var topUsedFields = usageStats
            .Where(s => s.UsageCount > 0)
            .OrderByDescending(s => s.UsageCount)
            .Take(10);

        result.AppendLine("## Top 10 Most Used Fields");
        result.AppendLine("| Field | Type | Usage Count | Usage % |");
        result.AppendLine("|-------|------|-------------|---------|");
        foreach (var field in topUsedFields)
        {
            var usagePercent = (field.UsageCount / (double)queries.Length) * 100;
            result.AppendLine($"| {field.FieldName} | {field.TypeName} | {field.UsageCount} | {usagePercent:F1}% |");
        }

        result.AppendLine();

        if (showUnused)
        {
            var unusedFields = usageStats
                .Where(s => s.UsageCount == 0)
                .OrderBy(s => s.TypeName)
                .ThenBy(s => s.FieldName);

            if (unusedFields.Any())
            {
                result.AppendLine("## Unused Fields");
                result.AppendLine("*These fields are defined in the schema but never used in the analyzed queries*\n");

                var groupedByType = unusedFields.GroupBy(f => f.TypeName);
                foreach (var typeGroup in groupedByType)
                {
                    result.AppendLine($"### {typeGroup.Key}");
                    foreach (var field in typeGroup)
                    {
                        result.AppendLine($"- `{field.FieldName}` ({field.FieldType})");
                    }

                    result.AppendLine();
                }
            }
        }

        var deprecationCandidates = usageStats
            .Where(s => s.UsageCount > 0 && s.UsageCount < queries.Length * 0.05)
            .OrderBy(s => s.UsageCount);

        if (deprecationCandidates.Any())
        {
            result.AppendLine("## Deprecation Candidates");
            result.AppendLine("*Fields with very low usage (< 5%) that might be candidates for deprecation*\n");
            foreach (var field in deprecationCandidates)
            {
                var usagePercent = (field.UsageCount / (double)queries.Length) * 100;
                result.AppendLine($"- `{field.TypeName}.{field.FieldName}` - {usagePercent:F1}% usage ({field.UsageCount}/{queries.Length})");
            }
            result.AppendLine();
        }

        result.AppendLine("## Usage Patterns");
        var patterns = AnalyzeUsagePatterns(usageStats, queries.Length);
        foreach (var pattern in patterns)
        {
            result.AppendLine($"- {pattern}");
        }

        return result.ToString();
    }

    [McpServerTool, Description("Create detailed reports showing which GraphQL fields are used in specific queries")]
    public static string GenerateUsageReport(
        [Description("GraphQL query to analyze usage patterns")]
        string query,
        [Description("Include field complexity analysis")]
        bool includeComplexity = true)
    {
        var result = new StringBuilder();
        result.AppendLine("# Query Field Usage Report\n");

        // Extract field usage from query
        var fieldUsage = ExtractFieldUsageFromQuery(query);

        result.AppendLine("## Field Selection Analysis");
        result.AppendLine($"- **Total Fields Selected:** {fieldUsage.Count}");
        result.AppendLine($"- **Unique Field Names:** {fieldUsage.Select(f => f.FieldName).Distinct().Count()}");
        result.AppendLine($"- **Max Nesting Depth:** {fieldUsage.Max(f => f.Depth)}");
        result.AppendLine($"- **Average Depth:** {fieldUsage.Average(f => f.Depth):F1}\n");

        // Fields by type
        var fieldsByType = fieldUsage.GroupBy(f => f.ParentType);
        result.AppendLine("## Fields by Type");
        foreach (var typeGroup in fieldsByType)
        {
            result.AppendLine($"### {typeGroup.Key}");
            foreach (var field in typeGroup.OrderBy(f => f.FieldName))
            {
                result.AppendLine($"- `{field.FieldName}` (depth: {field.Depth})");
            }

            result.AppendLine();
        }

        // Complexity analysis
        if (includeComplexity)
        {
            result.AppendLine("## Complexity Analysis");
            var complexity = CalculateFieldComplexity(fieldUsage);
            result.AppendLine($"- **Estimated Query Complexity:** {complexity.TotalComplexity}");
            result.AppendLine($"- **Complexity per Field:** {complexity.AverageComplexity:F1}");
            result.AppendLine($"- **Highest Complexity Field:** {complexity.MostComplexField}");

            if (complexity.HighComplexityFields.Any())
            {
                result.AppendLine("\n**High Complexity Fields:**");
                foreach (var field in complexity.HighComplexityFields)
                {
                    result.AppendLine($"- {field}");
                }
            }

            result.AppendLine();
        }

        // Usage recommendations
        var recommendations = GenerateUsageRecommendations(fieldUsage);
        if (recommendations.Any())
        {
            result.AppendLine("## Recommendations");
            foreach (var rec in recommendations)
            {
                result.AppendLine($"- {rec}");
            }
        }

        return result.ToString();
    }

    [McpServerTool, Description("Compare field selection patterns between two GraphQL queries to identify optimization opportunities")]
    public static string CompareFieldUsage(
        [Description("First GraphQL query")] string query1,
        [Description("Second GraphQL query")] string query2,
        [Description("Include optimization suggestions")]
        bool includeOptimizations = true)
    {
        var result = new StringBuilder();
        result.AppendLine("# Field Usage Comparison Report\n");

        var usage1 = ExtractFieldUsageFromQuery(query1);
        var usage2 = ExtractFieldUsageFromQuery(query2);

        var fields1 = usage1.Select(f => $"{f.ParentType}.{f.FieldName}")
            .ToHashSet();
        var fields2 = usage2.Select(f => $"{f.ParentType}.{f.FieldName}")
            .ToHashSet();

        // Comparison metrics
        result.AppendLine("## Comparison Metrics");
        result.AppendLine($"| Metric | Query 1 | Query 2 |");
        result.AppendLine($"|--------|---------|---------|");
        result.AppendLine($"| Total Fields | {usage1.Count} | {usage2.Count} |");
        result.AppendLine($"| Unique Fields | {fields1.Count} | {fields2.Count} |");
        result.AppendLine($"| Max Depth | {usage1.Max(f => f.Depth)} | {usage2.Max(f => f.Depth)} |");
        result.AppendLine($"| Avg Depth | {usage1.Average(f => f.Depth):F1} | {usage2.Average(f => f.Depth):F1} |\n");

        // Field overlap analysis
        var commonFields = fields1.Intersect(fields2);
        var query1OnlyFields = fields1.Except(fields2);
        var query2OnlyFields = fields2.Except(fields1);

        result.AppendLine("## Field Overlap Analysis");
        result.AppendLine($"- **Common Fields:** {commonFields.Count()}");
        result.AppendLine($"- **Query 1 Only:** {query1OnlyFields.Count()}");
        result.AppendLine($"- **Query 2 Only:** {query2OnlyFields.Count()}");
        result.AppendLine($"- **Overlap Percentage:** {(commonFields.Count() / (double)fields1.Union(fields2).Count()):P1}\n");

        if (commonFields.Any())
        {
            result.AppendLine("### Common Fields");
            foreach (var field in commonFields.OrderBy(f => f))
            {
                result.AppendLine($"- {field}");
            }

            result.AppendLine();
        }

        if (query1OnlyFields.Any())
        {
            result.AppendLine("### Query 1 Unique Fields");
            foreach (var field in query1OnlyFields.OrderBy(f => f))
            {
                result.AppendLine($"- {field}");
            }

            result.AppendLine();
        }

        if (query2OnlyFields.Any())
        {
            result.AppendLine("### Query 2 Unique Fields");
            foreach (var field in query2OnlyFields.OrderBy(f => f))
            {
                result.AppendLine($"- {field}");
            }

            result.AppendLine();
        }

        // Optimization suggestions
        if (includeOptimizations)
        {
            result.AppendLine("## Optimization Opportunities");
            var optimizations = GenerateOptimizationSuggestions(usage1, usage2, commonFields);
            foreach (var opt in optimizations)
            {
                result.AppendLine($"- {opt}");
            }
        }

        return result.ToString();
    }

    [McpServerTool, Description("Perform comprehensive field usage analytics with pattern recognition, performance correlation, schema optimization recommendations, and usage trend analysis. This advanced analytics tool provides deep insights for API governance and strategic planning.")]
    public static async Task<string> AnalyzeFieldUsageComprehensive(
        [Description("Log of executed queries as JSON array. Example: [\"query { users { id name } }\", \"query { posts { title author { name } } }\"]")]
        string queryLog,
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Analysis focus: 'optimization' for performance insights, 'governance' for schema management, 'trends' for usage patterns")]
        string analysisFocus = "optimization",
        [Description("Include predictive analytics and trend forecasting")]
        bool includePredictiveAnalytics = true,
        [Description("Include performance correlation analysis")]
        bool includePerformanceCorrelation = true,
        [Description("Time period for trend analysis in days")]
        int trendAnalysisPeriod = 30)
    {
        try
        {
            return await ServiceLocator.ExecuteWithSmartResponseServiceAsync(async smartResponseService =>
            {
                var smartResponse = await smartResponseService.CreateFieldUsageAnalyticsResponseAsync(
                    queryLog, endpointName, analysisFocus, includePredictiveAnalytics, includePerformanceCorrelation, trendAnalysisPeriod);
                
                return await smartResponseService.FormatComprehensiveResponseAsync(smartResponse);
            });
        }
        catch (Exception ex)
        {
            return await ServiceLocator.ExecuteWithSmartResponseServiceAsync(async smartResponseService =>
            {
                return await smartResponseService.CreateErrorResponseAsync(
                    "FieldUsageAnalyticsError", 
                    ex.Message,
                    new { queryLog, endpointName, analysisFocus });
            });
        }
    }

    private static async Task<List<SchemaField>> ExtractSchemaFields(string schemaJson)
    {
        var fields = new List<SchemaField>();

        try
        {
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);
            if (schemaData.TryGetProperty("data", out var data) &&
                data.TryGetProperty("__schema", out var schema) &&
                schema.TryGetProperty("types", out var types))
            {
                foreach (var type in types.EnumerateArray())
                {
                    if (type.TryGetProperty("name", out var typeNameElement) &&
                        type.TryGetProperty("fields", out var fieldsArray))
                    {
                        var typeName = typeNameElement.GetString();
                        if (!string.IsNullOrEmpty(typeName) && !typeName.StartsWith("__"))
                        {
                            foreach (var field in fieldsArray.EnumerateArray())
                            {
                                if (field.TryGetProperty("name", out var fieldNameElement) &&
                                    field.TryGetProperty("type", out var fieldTypeElement))
                                {
                                    fields.Add(new SchemaField
                                    {
                                        TypeName = typeName,
                                        FieldName = fieldNameElement.GetString() ?? "",
                                        FieldType = GraphQlTypeHelpers.GetTypeName(fieldTypeElement)
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Log error but continue
        }

        return fields;
    }

    private static List<FieldUsageStats> AnalyzeFieldUsageFromQueries(string[] queries, List<SchemaField> schemaFields)
    {
        var usageStats = schemaFields.Select(f => new FieldUsageStats
            {
                TypeName = f.TypeName,
                FieldName = f.FieldName,
                FieldType = f.FieldType,
                UsageCount = 0
            })
            .ToList();

        foreach (var query in queries)
        {
            var usedFields = ExtractFieldUsageFromQuery(query);

            foreach (var usedField in usedFields)
            {
                var stat = usageStats.FirstOrDefault(s =>
                    s.TypeName == usedField.ParentType &&
                    s.FieldName == usedField.FieldName);

                if (stat != null)
                {
                    stat.UsageCount++;
                }
            }
        }

        return usageStats;
    }

    private static List<FieldUsage> ExtractFieldUsageFromQuery(string query)
    {
        var fieldUsages = new List<FieldUsage>();

        // Simplified field extraction - would need more sophisticated parsing for production
        var fieldMatches = Regex.Matches(query, @"(\w+)\s*\{", RegexOptions.IgnoreCase);
        var depth = 0;
        var typeStack = new Stack<string>();
        typeStack.Push("Query"); // Default root type

        foreach (var c in query)
        {
            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (typeStack.Count > 1)
                {
                    typeStack.Pop();
                }
            }
        }

        // Reset for actual parsing
        depth = 0;
        typeStack.Clear();
        typeStack.Push("Query");

        var fieldMatches2 = Regex.Matches(query, @"\b(\w+)(?:\s*\{|\s*\(|\s*$)", RegexOptions.IgnoreCase);
        foreach (Match match in fieldMatches2)
        {
            var fieldName = match.Groups[1].Value;
            if (!IsGraphQlKeyword(fieldName))
            {
                fieldUsages.Add(new FieldUsage
                {
                    FieldName = fieldName,
                    ParentType = typeStack.Peek(),
                    Depth = Math.Max(1, depth)
                });
            }
        }

        return fieldUsages;
    }

    private static List<string> AnalyzeUsagePatterns(List<FieldUsageStats> usageStats, int totalQueries)
    {
        var patterns = new List<string>();

        var highUsageFields = usageStats.Count(s => s.UsageCount > totalQueries * 0.8);
        var mediumUsageFields = usageStats.Count(s => s.UsageCount > totalQueries * 0.2 && s.UsageCount <= totalQueries * 0.8);
        var lowUsageFields = usageStats.Count(s => s.UsageCount > 0 && s.UsageCount <= totalQueries * 0.2);

        patterns.Add($"High usage fields (>80%): {highUsageFields}");
        patterns.Add($"Medium usage fields (20-80%): {mediumUsageFields}");
        patterns.Add($"Low usage fields (<20%): {lowUsageFields}");

        if (highUsageFields > usageStats.Count * 0.1)
        {
            patterns.Add("Schema has good field utilization with many frequently used fields");
        }

        if (lowUsageFields > usageStats.Count * 0.5)
        {
            patterns.Add("Many fields have low usage - consider schema cleanup");
        }

        return patterns;
    }

    private static ComplexityAnalysis CalculateFieldComplexity(List<FieldUsage> fieldUsage)
    {
        var totalComplexity = fieldUsage.Sum(f => f.Depth);
        var averageComplexity = fieldUsage.Average(f => f.Depth);
        var maxComplexity = fieldUsage.Max(f => f.Depth);
        var mostComplexField = fieldUsage.FirstOrDefault(f => f.Depth == maxComplexity)
            ?.FieldName ?? "Unknown";

        var highComplexityFields = fieldUsage
            .Where(f => f.Depth > averageComplexity + 2)
            .Select(f => $"{f.ParentType}.{f.FieldName} (depth: {f.Depth})")
            .ToList();

        return new ComplexityAnalysis
        {
            TotalComplexity = totalComplexity,
            AverageComplexity = averageComplexity,
            MostComplexField = mostComplexField,
            HighComplexityFields = highComplexityFields
        };
    }

    private static List<string> GenerateUsageRecommendations(List<FieldUsage> fieldUsage)
    {
        var recommendations = new List<string>();

        var maxDepth = fieldUsage.Max(f => f.Depth);
        if (maxDepth > 5)
        {
            recommendations.Add($"Query depth ({maxDepth}) is quite deep - consider flattening the structure");
        }

        var duplicateFields = fieldUsage
            .GroupBy(f => f.FieldName)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        if (duplicateFields.Any())
        {
            recommendations.Add($"Duplicate field selections detected: {string.Join(", ", duplicateFields)}");
        }

        return recommendations;
    }

    private static List<string> GenerateOptimizationSuggestions(List<FieldUsage> usage1, List<FieldUsage> usage2, IEnumerable<string> commonFields)
    {
        var suggestions = new List<string>();

        if (commonFields.Count() > usage1.Count * 0.5 && commonFields.Count() > usage2.Count * 0.5)
        {
            suggestions.Add("High field overlap detected - consider creating a shared fragment");
        }

        var depth1 = usage1.Max(f => f.Depth);
        var depth2 = usage2.Max(f => f.Depth);

        if (Math.Abs(depth1 - depth2) > 3)
        {
            suggestions.Add("Significant depth difference - consider query optimization");
        }

        if (usage1.Count > usage2.Count * 2 || usage2.Count > usage1.Count * 2)
        {
            suggestions.Add("Significant field count difference - review if all fields are necessary");
        }

        return suggestions;
    }

    private static bool IsGraphQlKeyword(string word)
    {
        var keywords = new[] { "query", "mutation", "subscription", "fragment", "on", "true", "false", "null" };
        return keywords.Contains(word.ToLower());
    }
}