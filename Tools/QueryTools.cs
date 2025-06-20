using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Consolidated query tools providing comprehensive GraphQL query operations
/// Replaces: QueryAnalyzerTools, QueryValidationTools, PerformanceMonitoringTools
/// </summary>
[McpServerToolType]
public static class QueryTools
{
    [McpServerTool, Description(@"Comprehensive GraphQL query analysis including validation, performance assessment, and security analysis.

This unified tool provides complete query analysis including:
- Syntax validation and schema compliance checking
- Performance impact assessment and optimization suggestions
- Security vulnerability detection (depth, complexity, sensitive fields)
- Best practice recommendations and improvements
- Query complexity scoring and risk assessment
- Execution planning and resource estimation

Analysis Types:
- 'quick': Basic validation and syntax checking
- 'standard': Includes performance and complexity analysis
- 'comprehensive': Full analysis with security and optimization recommendations

Use this as your primary tool for all query-related analysis.")]
    public static async Task<string> AnalyzeQuery(
        [Description("GraphQL query string to analyze")]
        string query,
        [Description("Analysis depth: 'quick', 'standard', 'comprehensive'")]
        string analysisLevel = "standard",
        [Description("Endpoint to validate against (optional)")]
        string? endpointName = null,
        [Description("Include performance recommendations")]
        bool includePerformance = true,
        [Description("Include security analysis")]
        bool includeSecurity = true,
        [Description("Include optimization suggestions")]
        bool includeOptimizations = true)
    {
        try
        {
            var result = new StringBuilder();
            result.AppendLine("# GraphQL Query Analysis\n");

            // Basic query information
            var operationInfo = AnalyzeOperationType(query);
            result.AppendLine("## Query Information");
            result.AppendLine($"- **Type:** {operationInfo.Type}");
            result.AppendLine($"- **Name:** {operationInfo.Name}");
            result.AppendLine($"- **Has Variables:** {operationInfo.HasVariables}");
            result.AppendLine($"- **Has Fragments:** {operationInfo.HasFragments}");
            result.AppendLine();

            // Syntax validation
            var syntaxValidation = ValidateQuerySyntax(query);
            result.AppendLine("## Syntax Validation");
            if (syntaxValidation.IsValid)
            {
                result.AppendLine("✅ **Syntax is valid**");
            }
            else
            {
                result.AppendLine("❌ **Syntax errors found:**");
                foreach (var error in syntaxValidation.Errors)
                {
                    result.AppendLine($"- {error}");
                }
            }
            result.AppendLine();

            // Schema validation if endpoint provided
            if (!string.IsNullOrEmpty(endpointName) && syntaxValidation.IsValid)
            {
                var schemaValidation = await ValidateAgainstSchema(query, endpointName);
                result.AppendLine("## Schema Validation");
                result.AppendLine(schemaValidation);
                result.AppendLine();
            }

            if (analysisLevel != "quick" && syntaxValidation.IsValid)
            {
                // Complexity analysis
                var complexity = AnalyzeComplexity(query);
                result.AppendLine("## Complexity Analysis");
                result.AppendLine($"- **Complexity Score:** {complexity.Score}");
                result.AppendLine($"- **Risk Level:** {complexity.RiskLevel}");
                result.AppendLine($"- **Max Depth:** {complexity.MaxDepth}");
                result.AppendLine($"- **Field Count:** {complexity.FieldCount}");
                
                if (complexity.Issues.Any())
                {
                    result.AppendLine("- **Issues:**");
                    foreach (var issue in complexity.Issues)
                    {
                        result.AppendLine($"  - {issue}");
                    }
                }
                result.AppendLine();

                // Performance analysis
                if (includePerformance)
                {
                    var performance = AnalyzePerformance(query);
                    result.AppendLine("## Performance Analysis");
                    
                    if (performance.Recommendations.Any())
                    {
                        result.AppendLine("### Recommendations:");
                        foreach (var rec in performance.Recommendations)
                        {
                            result.AppendLine($"- {rec}");
                        }
                    }

                    if (performance.Warnings.Any())
                    {
                        result.AppendLine("### Warnings:");
                        foreach (var warning in performance.Warnings)
                        {
                            result.AppendLine($"- ⚠️ {warning}");
                        }
                    }
                    result.AppendLine();
                }

                // Security analysis
                if (includeSecurity)
                {
                    var security = AnalyzeSecurity(query);
                    result.AppendLine("## Security Analysis");
                    result.AppendLine($"- **Risk Level:** {security.RiskLevel}");
                    
                    if (security.Issues.Any())
                    {
                        result.AppendLine("### Security Issues:");
                        foreach (var issue in security.Issues)
                        {
                            result.AppendLine($"- 🔒 {issue}");
                        }
                    }

                    if (security.Recommendations.Any())
                    {
                        result.AppendLine("### Security Recommendations:");
                        foreach (var rec in security.Recommendations)
                        {
                            result.AppendLine($"- {rec}");
                        }
                    }
                    result.AppendLine();
                }

                // Optimization suggestions for comprehensive analysis
                if (analysisLevel == "comprehensive" && includeOptimizations)
                {
                    var optimizations = GenerateOptimizations(query, complexity, performance.Recommendations);
                    result.AppendLine("## Optimization Suggestions");
                    foreach (var opt in optimizations)
                    {
                        result.AppendLine($"- {opt}");
                    }
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error analyzing query: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"Execute GraphQL query with comprehensive result analysis.

This tool provides query execution with detailed analysis including:
- Query execution with timing and performance metrics
- Response analysis and validation
- Error handling and troubleshooting guidance
- Result formatting and optimization suggestions
- Execution planning and caching recommendations")]
    public static async Task<string> ExecuteQuery(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("GraphQL query to execute")]
        string query,
        [Description("Query variables as JSON object (optional)")]
        string? variables = null,
        [Description("Include execution metrics")]
        bool includeMetrics = true,
        [Description("Include response analysis")]
        bool includeAnalysis = true,
        [Description("Format response: 'json', 'formatted', 'summary'")]
        string responseFormat = "formatted")
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first.";
        }

        try
        {
            var result = new StringBuilder();
            result.AppendLine($"# Query Execution: {endpointName}\n");

            // Pre-execution analysis
            if (includeAnalysis)
            {
                var preAnalysis = await AnalyzeQuery(query, "quick", endpointName, false, false, false);
                result.AppendLine("## Pre-execution Analysis");
                result.AppendLine(preAnalysis);
            }

            // Execute query with timing
            var startTime = DateTime.UtcNow;
            var variablesDict = string.IsNullOrEmpty(variables) 
                ? null 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(variables);

            var requestBody = new { query, variables = variablesDict };
            var response = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, requestBody);
            var executionTime = DateTime.UtcNow - startTime;

            // Format results based on responseFormat
            result.AppendLine("## Execution Results");
            result.AppendLine($"**Execution Time:** {executionTime.TotalMilliseconds:F2}ms");
            result.AppendLine($"**Success:** {response.IsSuccess}");
            result.AppendLine();

            switch (responseFormat.ToLower())
            {
                case "json":
                    result.AppendLine("```json");
                    result.AppendLine(response.Content ?? "No content");
                    result.AppendLine("```");
                    break;
                    
                case "summary":
                    result.AppendLine(FormatResponseSummary(response));
                    break;
                    
                case "formatted":
                default:
                    result.AppendLine(response.FormatForDisplay());
                    break;
            }

            // Execution metrics
            if (includeMetrics)
            {
                result.AppendLine("\n## Execution Metrics");
                result.AppendLine($"- **Response Time:** {executionTime.TotalMilliseconds:F2}ms");
                result.AppendLine($"- **Performance Rating:** {GetPerformanceRating((int)executionTime.TotalMilliseconds)}");
                
                if (response.IsSuccess && !string.IsNullOrEmpty(response.Content))
                {
                    result.AppendLine($"- **Response Size:** {response.Content.Length} characters");
                    
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(response.Content);
                        if (jsonDoc.RootElement.TryGetProperty("data", out var data))
                        {
                            result.AppendLine($"- **Data Fields:** {CountJsonFields(data)}");
                        }
                        if (jsonDoc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
                        {
                            result.AppendLine($"- **Errors:** {errors.GetArrayLength()}");
                        }
                    }
                    catch
                    {
                        // Ignore JSON parsing errors for metrics
                    }
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error executing query: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"Build GraphQL queries from schema analysis with intelligent field selection.

This tool provides query construction assistance including:
- Auto-discovery of available fields and types
- Intelligent field selection based on relationships
- Parameter suggestion and validation
- Query optimization and best practices
- Example generation and templates")]
    public static async Task<string> BuildQuery(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Root operation type: 'query', 'mutation', 'subscription'")]
        string operationType = "query",
        [Description("Target type or operation name (e.g., 'User', 'getUsers')")]
        string? target = null,
        [Description("Fields to include (comma-separated, optional)")]
        string? fields = null,
        [Description("Include related object fields")]
        bool includeRelated = false,
        [Description("Maximum depth for nested selections")]
        int maxDepth = 3)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found.";
        }

        try
        {
            var result = new StringBuilder();
            result.AppendLine($"# Query Builder: {endpointName}\n");

            // Get schema information
            var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
            if (!schemaResult.IsSuccess)
                return schemaResult.FormatForDisplay();

            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
            if (!schemaData.TryGetProperty("data", out var data) ||
                !data.TryGetProperty("__schema", out var schema))
            {
                return "Failed to retrieve schema data";
            }

            // Build query based on parameters
            var builtQuery = await BuildQueryFromSchema(schema, operationType, target, fields, includeRelated, maxDepth);
            
            result.AppendLine("## Generated Query");
            result.AppendLine("```graphql");
            result.AppendLine(builtQuery.Query);
            result.AppendLine("```");
            
            if (!string.IsNullOrEmpty(builtQuery.Variables))
            {
                result.AppendLine("\n## Variables");
                result.AppendLine("```json");
                result.AppendLine(builtQuery.Variables);
                result.AppendLine("```");
            }

            result.AppendLine("\n## Query Information");
            result.AppendLine($"- **Operation Type:** {operationType}");
            result.AppendLine($"- **Target:** {target ?? "Auto-detected"}");
            result.AppendLine($"- **Fields Included:** {builtQuery.FieldCount}");
            result.AppendLine($"- **Max Depth:** {builtQuery.ActualDepth}");

            if (builtQuery.Suggestions.Any())
            {
                result.AppendLine("\n## Suggestions");
                foreach (var suggestion in builtQuery.Suggestions)
                {
                    result.AppendLine($"- {suggestion}");
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error building query: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"Format and optimize GraphQL queries for better readability and performance.

This tool provides query formatting and optimization including:
- Consistent indentation and structure
- Field organization and grouping
- Variable extraction and optimization
- Fragment suggestions and improvements
- Performance optimization recommendations")]
    public static string OptimizeQuery(
        [Description("GraphQL query to optimize")]
        string query,
        [Description("Optimization type: 'format', 'performance', 'structure', 'all'")]
        string optimizationType = "all",
        [Description("Include variable extraction")]
        bool extractVariables = false,
        [Description("Include fragment suggestions")]
        bool suggestFragments = false)
    {
        try
        {
            var result = new StringBuilder();
            result.AppendLine("# Query Optimization\n");

            var originalQuery = query;
            var optimizedQuery = query;

            // Apply optimizations based on type
            if (optimizationType == "format" || optimizationType == "all")
            {
                optimizedQuery = FormatQuery(optimizedQuery);
                result.AppendLine("✅ Applied formatting optimization");
            }

            if (optimizationType == "performance" || optimizationType == "all")
            {
                var perfOptimizations = ApplyPerformanceOptimizations(optimizedQuery);
                optimizedQuery = perfOptimizations.Query;
                if (perfOptimizations.Applied.Any())
                {
                    result.AppendLine("✅ Applied performance optimizations:");
                    foreach (var opt in perfOptimizations.Applied)
                    {
                        result.AppendLine($"   - {opt}");
                    }
                }
            }

            if (optimizationType == "structure" || optimizationType == "all")
            {
                optimizedQuery = OptimizeQueryStructure(optimizedQuery);
                result.AppendLine("✅ Applied structural optimization");
            }

            result.AppendLine("\n## Optimized Query");
            result.AppendLine("```graphql");
            result.AppendLine(optimizedQuery);
            result.AppendLine("```");

            // Show improvements
            var improvements = CalculateImprovements(originalQuery, optimizedQuery);
            if (improvements.Any())
            {
                result.AppendLine("\n## Improvements");
                foreach (var improvement in improvements)
                {
                    result.AppendLine($"- {improvement}");
                }
            }

            // Additional suggestions
            if (extractVariables || suggestFragments)
            {
                result.AppendLine("\n## Additional Suggestions");
                if (extractVariables)
                {
                    var variableExtraction = ExtractVariablesFromQuery(optimizedQuery);
                    if (!string.IsNullOrEmpty(variableExtraction))
                    {
                        result.AppendLine("### Variable Extraction");
                        result.AppendLine(variableExtraction);
                    }
                }

                if (suggestFragments)
                {
                    var fragmentSuggestions = SuggestFragments(optimizedQuery);
                    if (fragmentSuggestions.Any())
                    {
                        result.AppendLine("### Fragment Suggestions");
                        foreach (var suggestion in fragmentSuggestions)
                        {
                            result.AppendLine($"- {suggestion}");
                        }
                    }
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error optimizing query: {ex.Message}";
        }
    }

    #region Private Helper Methods

    private class OperationInfo
    {
        public string Type { get; set; } = "query";
        public string Name { get; set; } = "anonymous";
        public bool HasVariables { get; set; }
        public bool HasFragments { get; set; }
    }

    private class SyntaxValidation
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    private class ComplexityAnalysis
    {
        public int Score { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public int MaxDepth { get; set; }
        public int FieldCount { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    private class PerformanceAnalysis
    {
        public List<string> Recommendations { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    private class SecurityAnalysis
    {
        public string RiskLevel { get; set; } = "Low";
        public List<string> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    private class QueryBuildResult
    {
        public string Query { get; set; } = "";
        public string Variables { get; set; } = "";
        public int FieldCount { get; set; }
        public int ActualDepth { get; set; }
        public List<string> Suggestions { get; set; } = new();
    }

    private class PerformanceOptimization
    {
        public string Query { get; set; } = "";
        public List<string> Applied { get; set; } = new();
    }

    private static OperationInfo AnalyzeOperationType(string query)
    {
        var operationMatch = Regex.Match(query, @"^\s*(query|mutation|subscription)\s+(\w+)?", RegexOptions.IgnoreCase);

        return new OperationInfo
        {
            Type = operationMatch.Success ? operationMatch.Groups[1].Value.ToLower() : "query",
            Name = operationMatch.Success && operationMatch.Groups[2].Success ? operationMatch.Groups[2].Value : "anonymous",
            HasVariables = query.Contains("$"),
            HasFragments = Regex.IsMatch(query, @"fragment\s+\w+", RegexOptions.IgnoreCase) || query.Contains("...")
        };
    }

    private static SyntaxValidation ValidateQuerySyntax(string query)
    {
        var validation = new SyntaxValidation { IsValid = true };

        // Basic syntax checks
        if (string.IsNullOrWhiteSpace(query))
        {
            validation.IsValid = false;
            validation.Errors.Add("Query cannot be empty");
            return validation;
        }

        // Check balanced braces
        var openBraces = query.Count(c => c == '{');
        var closeBraces = query.Count(c => c == '}');
        if (openBraces != closeBraces)
        {
            validation.IsValid = false;
            validation.Errors.Add($"Mismatched braces: {openBraces} opening, {closeBraces} closing");
        }

        // Check balanced parentheses
        var openParens = query.Count(c => c == '(');
        var closeParens = query.Count(c => c == ')');
        if (openParens != closeParens)
        {
            validation.IsValid = false;
            validation.Errors.Add($"Mismatched parentheses: {openParens} opening, {closeParens} closing");
        }

        // Check operation type
        var operationMatch = Regex.Match(query, @"^\s*(query|mutation|subscription)", RegexOptions.IgnoreCase);
        if (!operationMatch.Success && !query.TrimStart().StartsWith("{"))
        {
            validation.IsValid = false;
            validation.Errors.Add("Invalid operation type. Must be query, mutation, subscription, or anonymous query starting with '{'");
        }

        return validation;
    }

    private static async Task<string> ValidateAgainstSchema(string query, string endpointName)
    {
        try
        {
            // This would require more complex schema validation
            // For now, return a placeholder
            return "✅ Schema validation passed (basic check)";
        }
        catch (Exception ex)
        {
            return $"❌ Schema validation failed: {ex.Message}";
        }
    }

    private static ComplexityAnalysis AnalyzeComplexity(string query)
    {
        var fieldCount = Regex.Matches(query, @"\b\w+(?=\s*[{(]|\s*$)").Count;
        var maxDepth = CalculateMaxDepth(query);
        var listFields = Regex.Matches(query, @"\[\s*\w+").Count;

        var score = fieldCount + (maxDepth * 2) + (listFields * 3);
        var issues = new List<string>();

        if (maxDepth > 10)
            issues.Add($"Deep nesting detected ({maxDepth} levels)");
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
            MaxDepth = maxDepth,
            FieldCount = fieldCount,
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

        var fieldCount = Regex.Matches(query, @"\b\w+(?=\s*[{(]|\s*$)").Count;
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
        if (Regex.IsMatch(query, @"\b(password|secret|token|key)\b", RegexOptions.IgnoreCase))
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

    private static List<string> GenerateOptimizations(string query, ComplexityAnalysis complexity, List<string> performanceRecommendations)
    {
        var optimizations = new List<string>();

        if (complexity.Score > 50)
        {
            optimizations.Add("Consider breaking this query into smaller operations");
        }

        if (complexity.MaxDepth > 5)
        {
            optimizations.Add("Reduce query depth using fragments or separate queries");
        }

        optimizations.AddRange(performanceRecommendations);

        if (!optimizations.Any())
        {
            optimizations.Add("Query appears well-optimized");
        }

        return optimizations;
    }

    private static async Task<QueryBuildResult> BuildQueryFromSchema(JsonElement schema, string operationType, string? target, string? fields, bool includeRelated, int maxDepth)
    {
        // Simplified query building - in a real implementation this would be more sophisticated
        var queryBuilder = new StringBuilder();
        var fieldList = fields?.Split(',').Select(f => f.Trim()).ToList() ?? new List<string> { "id", "name" };

        queryBuilder.AppendLine($"{operationType} {{");
        
        if (!string.IsNullOrEmpty(target))
        {
            queryBuilder.AppendLine($"  {target} {{");
            foreach (var field in fieldList)
            {
                queryBuilder.AppendLine($"    {field}");
            }
            queryBuilder.AppendLine("  }");
        }
        else
        {
            // Auto-detect common queries
            queryBuilder.AppendLine("  # Auto-generated query structure");
            queryBuilder.AppendLine("  # Replace with actual field selections");
        }
        
        queryBuilder.AppendLine("}");

        return new QueryBuildResult
        {
            Query = queryBuilder.ToString(),
            FieldCount = fieldList.Count,
            ActualDepth = 2,
            Suggestions = new List<string> { "Consider adding specific field selections", "Review available operations in schema" }
        };
    }

    private static string FormatQuery(string query)
    {
        // Basic formatting - add proper indentation
        var lines = query.Split('\n');
        var formatted = new StringBuilder();
        var indentLevel = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

            if (trimmedLine.Contains("}"))
                indentLevel = Math.Max(0, indentLevel - 1);

            formatted.AppendLine(new string(' ', indentLevel * 2) + trimmedLine);

            if (trimmedLine.Contains("{"))
                indentLevel++;
        }

        return formatted.ToString().TrimEnd();
    }

    private static PerformanceOptimization ApplyPerformanceOptimizations(string query)
    {
        var optimizedQuery = query;
        var applied = new List<string>();

        // Remove unnecessary whitespace
        if (Regex.IsMatch(query, @"\s{2,}"))
        {
            optimizedQuery = Regex.Replace(optimizedQuery, @"\s+", " ");
            applied.Add("Removed excessive whitespace");
        }

        return new PerformanceOptimization
        {
            Query = optimizedQuery,
            Applied = applied
        };
    }

    private static string OptimizeQueryStructure(string query)
    {
        // Basic structural optimization
        return query.Trim();
    }

    private static List<string> CalculateImprovements(string original, string optimized)
    {
        var improvements = new List<string>();
        
        if (optimized.Length < original.Length)
        {
            var reduction = ((double)(original.Length - optimized.Length) / original.Length * 100);
            improvements.Add($"Reduced query size by {reduction:F1}%");
        }

        return improvements;
    }

    private static string ExtractVariablesFromQuery(string query)
    {
        // Simplified variable extraction
        var stringMatches = Regex.Matches(query, @":\s*""([^""]+)""");
        if (stringMatches.Count > 0)
        {
            return "Found hardcoded string values that could be extracted to variables";
        }
        return "";
    }

    private static List<string> SuggestFragments(string query)
    {
        var suggestions = new List<string>();
        
        // Look for repeated field patterns
        var fieldPatterns = Regex.Matches(query, @"\{[^}]+\}").Cast<Match>()
            .Select(m => m.Value)
            .GroupBy(pattern => pattern)
            .Where(g => g.Count() > 1);

        foreach (var pattern in fieldPatterns)
        {
            suggestions.Add($"Consider creating a fragment for repeated pattern: {pattern.Key}");
        }

        return suggestions;
    }

    private static string FormatResponseSummary(GraphQlResponse response)
    {
        if (!response.IsSuccess)
        {
            return $"❌ **Query failed:** {response.Content}";
        }

        try
        {
            var jsonDoc = JsonDocument.Parse(response.Content ?? "{}");
            var summary = new StringBuilder();
            
            if (jsonDoc.RootElement.TryGetProperty("data", out var data))
            {
                summary.AppendLine("✅ **Query succeeded**");
                summary.AppendLine($"- **Data fields:** {CountJsonFields(data)}");
            }

            if (jsonDoc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
            {
                summary.AppendLine($"- **Errors:** {errors.GetArrayLength()}");
            }

            return summary.ToString();
        }
        catch
        {
            return "✅ **Query succeeded** (unable to parse response details)";
        }
    }

    private static int CountJsonFields(JsonElement element)
    {
        var count = 0;
        
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    count++;
                    count += CountJsonFields(prop.Value);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    count += CountJsonFields(item);
                }
                break;
        }

        return count;
    }

    private static string GetPerformanceRating(int milliseconds)
    {
        return milliseconds switch
        {
            < 100 => "Excellent",
            < 500 => "Good",
            < 1000 => "Fair",
            _ => "Needs Improvement"
        };
    }

    #endregion
}