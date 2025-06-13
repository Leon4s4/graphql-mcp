using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class QueryAnalyzerTools
{
    [McpServerTool, Description("Perform comprehensive analysis of GraphQL queries including complexity, performance impact, and best practice recommendations")]
    public static string AnalyzeQuery(
        [Description("GraphQL query to analyze")]
        string query,
        [Description("Include detailed complexity analysis")]
        bool includeComplexity = true,
        [Description("Include performance recommendations")]
        bool includePerformance = true,
        [Description("Include security analysis")]
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
        [Description("Name of the registered GraphQL endpoint")] string endpointName,
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
        var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

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