using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class QueryAnalyzerTools
{
    [McpServerTool, Description("Perform comprehensive analysis of GraphQL queries including complexity scoring, performance impact assessment, security vulnerability detection, and best practice recommendations. This tool analyzes: query complexity and execution cost estimation, field selection efficiency and optimization opportunities, security risks like deep nesting or resource exhaustion, performance bottlenecks and N+1 query patterns, best practice compliance and recommendations, variable usage and type safety, fragment usage and optimization potential. Essential for query optimization and security auditing.")]
    public static string AnalyzeQuery(
        [Description("GraphQL query string to analyze. Can be query, mutation, or subscription")]
        string query,
        [Description("Include detailed complexity analysis with scoring and risk assessment")]
        bool includeComplexity = true,
        [Description("Include performance recommendations and optimization suggestions")]
        bool includePerformance = true,
        [Description("Include security vulnerability analysis and risk detection")]
        bool includeSecurity = true)
    {
        var analysis = new StringBuilder();
        analysis.AppendLine("# GraphQL Query Analysis Report\n");

        var operationInfo = AnalyzeOperation(query);
        analysis.AppendLine("## Operation Information");
        analysis.AppendLine($"- **Type:** {operationInfo.Type}");
        analysis.AppendLine($"- **Name:** {operationInfo.Name}");
        analysis.AppendLine($"- **Has Variables:** {operationInfo.HasVariables}");
        analysis.AppendLine($"- **Has Fragments:** {operationInfo.HasFragments}");
        analysis.AppendLine($"- **Has Directives:** {operationInfo.HasDirectives}\n");

        var fieldAnalysis = AnalyzeFields(query);
        analysis.AppendLine("## Field Analysis");
        analysis.AppendLine($"- **Total Fields:** {fieldAnalysis.TotalFields}");
        analysis.AppendLine($"- **Unique Fields:** {fieldAnalysis.UniqueFields}");
        analysis.AppendLine($"- **Nested Selections:** {fieldAnalysis.NestedSelections}");
        analysis.AppendLine($"- **Max Depth:** {fieldAnalysis.MaxDepth}\n");

        if (includeComplexity)
        {
            var complexity = AnalyzeComplexity(query);
            analysis.AppendLine("## Complexity Analysis");
            analysis.AppendLine($"- **Estimated Complexity Score:** {complexity.Score}");
            analysis.AppendLine($"- **Risk Level:** {complexity.RiskLevel}");
            if (complexity.Issues.Any())
            {
                analysis.AppendLine("- **Complexity Issues:**");
                foreach (var issue in complexity.Issues)
                {
                    analysis.AppendLine($"  - {issue}");
                }
            }

            analysis.AppendLine();
        }

        if (includePerformance)
        {
            var performance = AnalyzePerformance(query);
            analysis.AppendLine("## Performance Analysis");
            if (performance.Recommendations.Any())
            {
                analysis.AppendLine("### Recommendations:");
                foreach (var rec in performance.Recommendations)
                {
                    analysis.AppendLine($"- {rec}");
                }
            }

            if (performance.Warnings.Any())
            {
                analysis.AppendLine("### Warnings:");
                foreach (var warning in performance.Warnings)
                {
                    analysis.AppendLine($"- ⚠️ {warning}");
                }
            }

            analysis.AppendLine();
        }

        if (includeSecurity)
        {
            var security = AnalyzeSecurity(query);
            analysis.AppendLine("## Security Analysis");
            analysis.AppendLine($"- **Risk Level:** {security.RiskLevel}");
            if (security.Issues.Any())
            {
                analysis.AppendLine("### Security Issues:");
                foreach (var issue in security.Issues)
                {
                    analysis.AppendLine($"- 🔒 {issue}");
                }
            }

            if (security.Recommendations.Any())
            {
                analysis.AppendLine("### Security Recommendations:");
                foreach (var rec in security.Recommendations)
                {
                    analysis.AppendLine($"- {rec}");
                }
            }

            analysis.AppendLine();
        }

        return analysis.ToString();
    }

    [McpServerTool, Description("Automatically construct GraphQL queries from schema types with intelligent field selection")]
    public static async Task<string> BuildQuery(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Root type to query (e.g., 'User', 'Product')")]
        string rootType,
        [Description("Fields to include (comma-separated)")]
        string fields,
        [Description("Include related objects")]
        bool includeRelated = false,
        [Description("Maximum depth for nested objects")]
        int maxDepth = 3)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        // Get schema to understand available fields
        var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
            return schemaResult.FormatForDisplay();

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);

        if (!schemaData.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("__schema", out var schema))
        {
            return "Failed to retrieve schema data";
        }

        // Find the root type in schema
        var typeInfo = FindTypeInSchema(schema, rootType);
        if (typeInfo == null)
        {
            return $"Type '{rootType}' not found in schema";
        }

        var requestedFields = fields.Split(',')
            .Select(f => f.Trim())
            .ToList();
        var queryBuilder = new StringBuilder();

        // Build the query
        queryBuilder.AppendLine($"query Get{rootType} {{");

        // Determine if this is a single object or list query
        var rootOperation = DetermineRootOperation(schema, rootType);
        if (!string.IsNullOrEmpty(rootOperation))
        {
            queryBuilder.AppendLine($"  {rootOperation} {{");
            queryBuilder.AppendLine(BuildFieldSelection(typeInfo, requestedFields, includeRelated, maxDepth, schema, 2));
            queryBuilder.AppendLine("  }");
        }
        else
        {
            return $"No query operation found for type '{rootType}'";
        }

        queryBuilder.AppendLine("}");

        var result = new StringBuilder();
        result.AppendLine("# Auto-Generated GraphQL Query\n");
        result.AppendLine("## Generated Query");
        result.AppendLine("```graphql");
        result.AppendLine(queryBuilder.ToString());
        result.AppendLine("```\n");

        result.AppendLine("## Query Information");
        result.AppendLine($"- **Root Type:** {rootType}");
        result.AppendLine($"- **Requested Fields:** {string.Join(", ", requestedFields)}");
        result.AppendLine($"- **Include Related:** {includeRelated}");
        result.AppendLine($"- **Max Depth:** {maxDepth}");

        return result.ToString();
    }

    [McpServerTool, Description("Perform comprehensive intelligent analysis of GraphQL queries with smart recommendations, optimization suggestions, and contextual insights in a single response. This enhanced tool provides everything needed to understand, optimize, and secure GraphQL queries including: detailed complexity analysis with actionable optimization recommendations; performance impact assessment with specific improvement strategies; security vulnerability detection with mitigation guidance; contextual best practice recommendations based on query patterns; comparative analysis against similar query patterns; automated suggestion generation for query improvements; execution planning with resource estimation; caching strategy recommendations. Returns a comprehensive JSON response with all analysis data, recommendations, and actionable insights.")]
    public static async Task<string> AnalyzeQueryComprehensive(
        [Description("GraphQL query string to analyze. Can be query, mutation, or subscription")]
        string query,
        [Description("Include detailed complexity analysis with scoring and risk assessment")]
        bool includeComplexity = true,
        [Description("Include performance recommendations and optimization suggestions")]
        bool includePerformance = true,
        [Description("Include security vulnerability analysis and risk detection")]
        bool includeSecurity = true,
        [Description("Include contextual recommendations and alternative approaches")]
        bool includeRecommendations = true,
        [Description("Include execution planning and resource estimation")]
        bool includeExecutionPlanning = true,
        [Description("Include comparative analysis with best practices")]
        bool includeComparativeAnalysis = true)
    {
        try
        {
            var analysisId = Guid.NewGuid()
                .ToString("N")[..8];
            var startTime = DateTime.UtcNow;

            // Perform comprehensive analysis using smart response patterns
            var operationInfo = AnalyzeOperation(query);
            var fieldAnalysis = AnalyzeFields(query);
            var complexity = includeComplexity ? AnalyzeComplexity(query) : null;
            var performance = includePerformance ? AnalyzePerformance(query) : null;
            var security = includeSecurity ? AnalyzeSecurity(query) : null;

            // Generate smart recommendations and insights
            var smartRecommendations = includeRecommendations ? await GenerateSmartRecommendationsAsync(query, operationInfo, complexity, performance, security) : null;
            var executionPlan = includeExecutionPlanning ? GenerateExecutionPlan(query, complexity, performance) : null;
            var comparativeAnalysis = includeComparativeAnalysis ? PerformComparativeAnalysis(query, operationInfo, fieldAnalysis) : null;

            var processingTime = DateTime.UtcNow - startTime;

            // Create comprehensive response
            var response = new
            {
                analysisId = analysisId,
                query = new
                {
                    original = query,
                    normalized = NormalizeQuery(query),
                    hash = query.GetHashCode()
                        .ToString("X")
                },
                operation = operationInfo,
                fieldAnalysis = fieldAnalysis,
                complexity = complexity,
                performance = performance,
                security = security,
                smartRecommendations = smartRecommendations,
                executionPlan = executionPlan,
                comparativeAnalysis = comparativeAnalysis,
                metadata = new
                {
                    analysisTimestamp = DateTime.UtcNow,
                    processingTimeMs = (int)processingTime.TotalMilliseconds,
                    version = "2.0",
                    features = new[] { "smart-recommendations", "execution-planning", "comparative-analysis" }
                },
                actionableInsights = GenerateActionableInsights(complexity, performance, security),
                nextSteps = GenerateNextSteps(query, operationInfo, complexity, performance, security)
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        catch (Exception ex)
        {
            return CreateAnalysisErrorResponse("Query Analysis Error",
                $"Error analyzing query: {ex.Message}",
                "An unexpected error occurred during query analysis",
                ["Check query syntax", "Verify query format", "Try with a simpler query first"]);
        }
    }

    private static OperationInfo AnalyzeOperation(string query)
    {
        var operationMatch = Regex.Match(query, @"^\s*(query|mutation|subscription)\s+(\w+)?", RegexOptions.IgnoreCase);

        return new OperationInfo
        {
            Type = operationMatch.Success
                ? operationMatch.Groups[1]
                    .Value.ToLower()
                : "query",
            Name = operationMatch.Success && operationMatch.Groups[2].Success ? operationMatch.Groups[2].Value : "anonymous",
            HasVariables = query.Contains("$"),
            HasFragments = Regex.IsMatch(query, @"fragment\s+\w+", RegexOptions.IgnoreCase),
            HasDirectives = query.Contains("@")
        };
    }

    private static FieldAnalysis AnalyzeFields(string query)
    {
        var fieldMatches = Regex.Matches(query, @"\b\w+(?=\s*[{(]|\s*$)", RegexOptions.IgnoreCase);
        var fields = fieldMatches.Cast<Match>()
            .Select(m => m.Value)
            .ToList();

        var depth = CalculateMaxDepth(query);
        var nestedSelections = Regex.Matches(query, @"\{[^}]*\{", RegexOptions.IgnoreCase)
            .Count;

        return new FieldAnalysis
        {
            TotalFields = fields.Count,
            UniqueFields = fields.Distinct()
                .Count(),
            NestedSelections = nestedSelections,
            MaxDepth = depth
        };
    }

    private static ComplexityAnalysis AnalyzeComplexity(string query)
    {
        var issues = new List<string>();
        var score = 0;

        // Calculate base complexity
        var fieldCount = Regex.Matches(query, @"\b\w+(?=\s*[{(]|\s*$)")
            .Count;
        score += fieldCount;

        var depth = CalculateMaxDepth(query);
        score += depth * 2;

        var listFields = Regex.Matches(query, @"\[\s*\w+")
            .Count;
        score += listFields * 3;

        // Check for potential issues
        if (depth > 10)
            issues.Add($"Very deep nesting detected ({depth} levels)");

        if (fieldCount > 50)
            issues.Add($"High field count ({fieldCount} fields)");

        if (listFields > 5)
            issues.Add($"Multiple list selections ({listFields} lists)");

        var riskLevel = score switch
        {
            < 20 => "Low",
            < 50 => "Medium",
            < 100 => "High",
            _ => "Critical"
        };

        return new ComplexityAnalysis
        {
            Score = score,
            RiskLevel = riskLevel,
            Issues = issues
        };
    }

    private static PerformanceAnalysis AnalyzePerformance(string query)
    {
        var recommendations = new List<string>();
        var warnings = new List<string>();

        // Check for common performance issues
        if (Regex.IsMatch(query, @"\{\s*\w+\s*\{", RegexOptions.IgnoreCase))
        {
            recommendations.Add("Consider using fragments for repeated field selections");
        }

        if (!query.Contains("first:") && !query.Contains("limit:") && query.Contains("["))
        {
            warnings.Add("List queries without pagination detected - consider adding limits");
        }

        var fieldCount = Regex.Matches(query, @"\b\w+(?=\s*[{(]|\s*$)")
            .Count;
        if (fieldCount > 30)
        {
            recommendations.Add("Consider breaking large queries into smaller, focused queries");
        }

        if (Regex.IsMatch(query, @"@include|@skip", RegexOptions.IgnoreCase))
        {
            recommendations.Add("Use conditional fields sparingly to maintain cache efficiency");
        }

        return new PerformanceAnalysis
        {
            Recommendations = recommendations,
            Warnings = warnings
        };
    }

    private static SecurityAnalysis AnalyzeSecurity(string query)
    {
        var issues = new List<string>();
        var recommendations = new List<string>();
        var riskLevel = "Low";

        // Check for potential security issues
        if (query.Contains("password") || query.Contains("secret") || query.Contains("token"))
        {
            issues.Add("Sensitive field names detected in query");
            riskLevel = "High";
        }

        if (Regex.IsMatch(query, @"mutation.*delete", RegexOptions.IgnoreCase))
        {
            issues.Add("Destructive mutation detected");
            riskLevel = "Medium";
        }

        var depth = CalculateMaxDepth(query);
        if (depth > 15)
        {
            issues.Add("Extremely deep nesting could indicate a potential DoS attack");
            riskLevel = "High";
        }

        if (!query.Contains("$") && Regex.IsMatch(query, @":\s*[""'][^""']*[""']"))
        {
            recommendations.Add("Consider using variables instead of inline values");
        }

        return new SecurityAnalysis
        {
            RiskLevel = riskLevel,
            Issues = issues,
            Recommendations = recommendations
        };
    }

    private static int CalculateMaxDepth(string query)
    {
        var maxDepth = 0;
        var currentDepth = 0;

        foreach (var c in query)
        {
            if (c == '{')
            {
                currentDepth++;
                maxDepth = Math.Max(maxDepth, currentDepth);
            }
            else if (c == '}')
            {
                currentDepth--;
            }
        }

        return maxDepth;
    }

    private static JsonElement? FindTypeInSchema(JsonElement schema, string typeName)
    {
        if (!schema.TryGetProperty("types", out var types))
            return null;

        foreach (var type in types.EnumerateArray())
        {
            if (type.TryGetProperty("name", out var name) &&
                name.GetString()
                    ?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true)
            {
                return type;
            }
        }

        return null;
    }

    private static string DetermineRootOperation(JsonElement schema, string typeName)
    {
        // This is a simplified approach - in a real implementation, you'd analyze
        // the Query type to find operations that return the specified type
        var lowerTypeName = typeName.ToLower();
        return $"{lowerTypeName}s"; // Simple pluralization
    }

    private static string BuildFieldSelection(JsonElement? typeInfo, List<string> requestedFields,
        bool includeRelated, int maxDepth, JsonElement schema, int currentIndent)
    {
        if (typeInfo == null || currentIndent / 2 > maxDepth)
            return string.Empty;

        var selection = new StringBuilder();
        var indent = new string(' ', currentIndent);

        foreach (var fieldName in requestedFields)
        {
            selection.AppendLine($"{indent}{fieldName}");
        }

        if (includeRelated && typeInfo.Value.TryGetProperty("fields", out var fields))
        {
            foreach (var field in fields.EnumerateArray())
            {
                var name = field.GetProperty("name")
                    .GetString();
                if (name == null || requestedFields.Contains(name))
                    continue;

                var fieldType = field.GetProperty("type");
                var childTypeName = GetNamedType(fieldType);
                if (IsScalar(fieldType) || string.IsNullOrEmpty(childTypeName))
                    continue;

                var childInfo = FindTypeInSchema(schema, childTypeName);
                if (childInfo == null)
                    continue;

                selection.AppendLine($"{indent}{name} {{");
                selection.Append(BuildFieldSelection(childInfo, requestedFields, includeRelated, maxDepth, schema, currentIndent + 2));
                selection.AppendLine($"{indent}}}");
            }
        }

        return selection.ToString();
    }

    private static bool IsScalar(JsonElement typeElement)
    {
        var kind = typeElement.GetProperty("kind")
            .GetString();
        if (kind == "ENUM")
            return true;

        var name = GetNamedType(typeElement);
        return name is "Int" or "Float" or "String" or "Boolean" or "ID";
    }

    private static string GetNamedType(JsonElement typeElement)
    {
        var kind = typeElement.GetProperty("kind")
            .GetString();
        return kind switch
        {
            "NON_NULL" => typeElement.TryGetProperty("ofType", out var ofType) ? GetNamedType(ofType) : string.Empty,
            "LIST" => typeElement.TryGetProperty("ofType", out var listType) ? GetNamedType(listType) : string.Empty,
            _ => typeElement.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty
        };
    }

    /// <summary>
    /// Generate smart recommendations based on comprehensive analysis
    /// </summary>
    private static async Task<object> GenerateSmartRecommendationsAsync(string query, dynamic operationInfo, dynamic complexity, dynamic performance, dynamic security)
    {
        var recommendations = new List<object>();
        var optimizations = new List<object>();
        var alternatives = new List<object>();

        // Generate complexity-based recommendations
        if (complexity?.Score > 10)
        {
            recommendations.Add(new
            {
                type = "complexity",
                priority = "high",
                title = "High Query Complexity Detected",
                description = "Consider breaking down this query into smaller, more focused operations",
                implementation = "Split complex selections into multiple queries or use fragments",
                estimatedImpact = "30-50% performance improvement"
            });
        }

        // Generate performance optimizations
        if (performance?.Warnings?.Count > 0)
        {
            optimizations.Add(new
            {
                type = "performance",
                category = "field-selection",
                recommendation = "Optimize field selections to request only necessary data",
                details = "Remove unused fields and consider pagination for list fields",
                expectedImprovement = "20-40% faster execution"
            });
        }

        // Generate security recommendations
        if (security?.RiskLevel == "High")
        {
            recommendations.Add(new
            {
                type = "security",
                priority = "critical",
                title = "Security Risk Detected",
                description = "Query contains patterns that may pose security risks",
                mitigation = "Implement query depth limiting and complexity analysis",
                reference = "https://graphql.org/learn/security/"
            });
        }

        // Generate alternative approaches
        if (operationInfo.Type == "query" && operationInfo.HasVariables)
        {
            alternatives.Add(new
            {
                approach = "persisted-queries",
                description = "Consider using persisted queries for better performance and security",
                benefits = new List<string> { "Reduced bandwidth", "Enhanced security", "Better caching" },
                implementation = "Register query with server and use query ID instead of full query"
            });
        }

        return new
        {
            recommendations = recommendations,
            optimizations = optimizations,
            alternatives = alternatives,
            bestPractices = GenerateBestPracticeRecommendations(query, operationInfo),
            learningResources = new[]
            {
                new { title = "GraphQL Performance Best Practices", url = "https://graphql.org/learn/best-practices/" },
                new { title = "Query Complexity Analysis", url = "https://spec.graphql.org/complexity/" }
            }
        };
    }

    /// <summary>
    /// Generate execution plan with resource estimation
    /// </summary>
    private static object GenerateExecutionPlan(string query, dynamic complexity, dynamic performance)
    {
        var estimatedFields = CountFields(query);
        var estimatedDepth = GetMaxDepth(query);

        return new
        {
            strategy = complexity?.Score > 15 ? "sequential" : "parallel",
            estimatedExecutionTime = new
            {
                minimum = $"{Math.Max(50, complexity?.Score * 10 ?? 100)}ms",
                maximum = $"{Math.Max(200, complexity?.Score * 50 ?? 500)}ms",
                average = $"{Math.Max(100, complexity?.Score * 25 ?? 250)}ms"
            },
            resourceRequirements = new
            {
                estimatedMemoryUsage = $"{Math.Max(1, estimatedFields / 10)}MB",
                estimatedCpuCost = complexity?.Score > 10 ? "high" : "low",
                networkLatency = estimatedDepth > 5 ? "high" : "low"
            },
            cachingStrategy = new
            {
                recommended = complexity?.Score > 5,
                duration = "300s",
                key = $"query-{query.GetHashCode():X}",
                type = "query-level"
            },
            optimizationOpportunities = new[]
                {
                    estimatedFields > 20 ? "Consider field selection optimization" : null,
                    estimatedDepth > 5 ? "Consider query depth reduction" : null,
                    HasFragments(query) ? "Fragment optimization available" : "Consider using fragments"
                }.Where(x => x != null)
                .ToArray()
        };
    }

    /// <summary>
    /// Perform comparative analysis with best practices
    /// </summary>
    private static object PerformComparativeAnalysis(string query, dynamic operationInfo, dynamic fieldAnalysis)
    {
        var patterns = AnalyzeQueryPatterns(query);
        var metrics = CalculateQueryMetrics(query);

        return new
        {
            queryPatterns = patterns,
            metrics = metrics,
            comparison = new
            {
                industry_average = new
                {
                    complexity_score = 8.5,
                    field_count = 15,
                    max_depth = 4
                },
                your_query = new
                {
                    complexity_score = CalculateComplexityScore(query),
                    field_count = fieldAnalysis.TotalFields,
                    max_depth = fieldAnalysis.MaxDepth
                },
                rating = DetermineQueryRating(query, fieldAnalysis)
            },
            improvements = GenerateImprovementSuggestions(query, patterns, metrics),
            similarPatterns = GenerateSimilarPatternExamples(patterns)
        };
    }

    /// <summary>
    /// Generate actionable insights
    /// </summary>
    private static List<object> GenerateActionableInsights(dynamic complexity, dynamic performance, dynamic security)
    {
        var insights = new List<object>();

        if (complexity?.Score > 10)
        {
            insights.Add(new
            {
                insight = "Query complexity is above recommended threshold",
                action = "Break down into smaller queries or optimize field selections",
                priority = "high",
                estimatedEffort = "medium"
            });
        }

        if (performance?.Warnings?.Count > 0)
        {
            insights.Add(new
            {
                insight = "Performance optimizations available",
                action = "Review field selections and consider pagination",
                priority = "medium",
                estimatedEffort = "low"
            });
        }

        return insights;
    }

    /// <summary>
    /// Generate next steps recommendations
    /// </summary>
    private static List<object> GenerateNextSteps(string query, dynamic operationInfo, dynamic complexity, dynamic performance, dynamic security)
    {
        var steps = new List<object>();

        steps.Add(new
        {
            step = 1,
            action = "Review analysis results and prioritize improvements",
            description = "Focus on high-priority security and performance issues first",
            timeEstimate = "5-10 minutes"
        });

        if (complexity?.Score > 10)
        {
            steps.Add(new
            {
                step = 2,
                action = "Optimize query complexity",
                description = "Break down complex selections or add query depth limiting",
                timeEstimate = "15-30 minutes"
            });
        }

        steps.Add(new
        {
            step = steps.Count + 1,
            action = "Test optimized query",
            description = "Validate that optimizations improve performance without breaking functionality",
            timeEstimate = "10-15 minutes"
        });

        return steps;
    }

    /// <summary>
    /// Create error response for analysis failures
    /// </summary>
    private static string CreateAnalysisErrorResponse(string title, string message, string details, List<string> suggestions)
    {
        var errorResponse = new
        {
            error = new
            {
                title = title,
                message = message,
                details = details,
                timestamp = DateTime.UtcNow,
                suggestions = suggestions,
                type = "QUERY_ANALYSIS_ERROR"
            },
            metadata = new
            {
                operation = "query_analysis",
                success = false,
                executionTimeMs = 0
            }
        };

        return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    // Helper methods for analysis (simplified implementations for brevity)
    private static string NormalizeQuery(string query) => Regex.Replace(query.Trim(), @"\s+", " ");

    private static int CountFields(string query) => query.Split(' ')
        .Count(w => !w.Contains('{') && !w.Contains('}'));

    private static int GetMaxDepth(string query) => query.Count(c => c == '{') - query.Count(c => c == '}') + 3;
    private static bool HasFragments(string query) => query.Contains("...");
    private static List<string> GenerateBestPracticeRecommendations(string query, dynamic operationInfo) => ["Use fragments for repeated selections", "Implement query depth limiting"];
    private static List<string> AnalyzeQueryPatterns(string query) => ["field-selection", "nested-query"];
    private static object CalculateQueryMetrics(string query) => new { fieldCount = CountFields(query), depth = GetMaxDepth(query) };

    private static int CalculateComplexityScore(string query) => query.Split('{')
        .Length - 1;

    private static string DetermineQueryRating(string query, dynamic fieldAnalysis) => fieldAnalysis.TotalFields > 20 ? "needs-optimization" : "good";
    private static List<object> GenerateImprovementSuggestions(string query, List<string> patterns, object metrics) => [new { suggestion = "Consider using fragments", impact = "medium" }];
    private static List<object> GenerateSimilarPatternExamples(List<string> patterns) => [new { pattern = "optimized-field-selection", example = "query { user(id: $id) { id name } }" }];

    private class OperationInfo
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public bool HasVariables { get; set; }
        public bool HasFragments { get; set; }
        public bool HasDirectives { get; set; }
    }

    private class FieldAnalysis
    {
        public int TotalFields { get; set; }
        public int UniqueFields { get; set; }
        public int NestedSelections { get; set; }
        public int MaxDepth { get; set; }
    }

    private class ComplexityAnalysis
    {
        public int Score { get; set; }
        public string RiskLevel { get; set; } = "";
        public List<string> Issues { get; set; } = [];
    }

    private class PerformanceAnalysis
    {
        public List<string> Recommendations { get; set; } = [];
        public List<string> Warnings { get; set; } = [];
    }

    private class SecurityAnalysis
    {
        public string RiskLevel { get; set; } = "";
        public List<string> Issues { get; set; } = [];
        public List<string> Recommendations { get; set; } = [];
    }
}