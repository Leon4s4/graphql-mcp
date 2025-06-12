using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class SecurityAnalysisTools
{
    [McpServerTool, Description("Analyze query for security issues and complexity")]
    public static async Task<string> AnalyzeQuerySecurity(
        [Description("GraphQL query to analyze")]
        string query,
        [Description("Schema endpoint")] string endpoint,
        [Description("Max query depth allowed")]
        int maxDepth = 10,
        [Description("Max query complexity")] int maxComplexity = 1000,
        [Description("HTTP headers as JSON object (optional)")]
        string? headers = null)
    {
        var result = new StringBuilder();
        result.AppendLine("# GraphQL Security Analysis Report\n");

        var complexityAnalysis = AnalyzeQueryComplexity(query, maxComplexity);
        result.AppendLine("## Query Complexity Analysis");
        result.AppendLine($"- **Estimated Complexity:** {complexityAnalysis.Score}");
        result.AppendLine($"- **Max Allowed:** {maxComplexity}");
        result.AppendLine($"- **Status:** {(complexityAnalysis.Score <= maxComplexity ? "✅ Within limits" : "❌ Exceeds limits")}");

        if (complexityAnalysis.Score > maxComplexity)
        {
            result.AppendLine($"- **Risk:** Query may cause server overload");
        }

        result.AppendLine();

        var depthAnalysis = AnalyzeQueryDepth(query, maxDepth);
        result.AppendLine("## Query Depth Analysis");
        result.AppendLine($"- **Actual Depth:** {depthAnalysis.ActualDepth}");
        result.AppendLine($"- **Max Allowed:** {maxDepth}");
        result.AppendLine($"- **Status:** {(depthAnalysis.ActualDepth <= maxDepth ? "✅ Within limits" : "❌ Exceeds limits")}");

        if (depthAnalysis.ActualDepth > maxDepth)
        {
            result.AppendLine($"- **Risk:** Deep nesting may cause performance issues or DoS attacks");
        }

        result.AppendLine();

        var introspectionRisks = DetectIntrospectionQueries(query);
        result.AppendLine("## Introspection Analysis");
        if (introspectionRisks.Any())
        {
            result.AppendLine("⚠️ **Introspection queries detected:**");
            foreach (var risk in introspectionRisks)
            {
                result.AppendLine($"- {risk}");
            }

            result.AppendLine("- **Recommendation:** Disable introspection in production");
        }
        else
        {
            result.AppendLine("✅ **Status:** No introspection queries detected");
        }

        result.AppendLine();

        var injectionRisks = DetectInjectionRisks(query);
        result.AppendLine("## Injection Risk Analysis");
        if (injectionRisks.Any())
        {
            result.AppendLine("⚠️ **Potential injection risks:**");
            foreach (var risk in injectionRisks)
            {
                result.AppendLine($"- {risk}");
            }
        }
        else
        {
            result.AppendLine("✅ **Status:** No obvious injection risks detected");
        }

        result.AppendLine();

        var resourceRisks = AnalyzeResourceConsumption(query);
        result.AppendLine("## Resource Consumption Analysis");
        if (resourceRisks.Any())
        {
            result.AppendLine("⚠️ **Resource consumption concerns:**");
            foreach (var risk in resourceRisks)
            {
                result.AppendLine($"- {risk}");
            }
        }
        else
        {
            result.AppendLine("✅ **Status:** No obvious resource consumption issues");
        }

        result.AppendLine();

        // 6. Schema-based Security Analysis (if schema available)
        try
        {
            var schemaAnalysis = await AnalyzeSchemaBasedSecurity(query, endpoint, headers);
            result.AppendLine("## Schema-based Security Analysis");
            result.AppendLine(schemaAnalysis);
        }
        catch (Exception ex)
        {
            result.AppendLine("## Schema-based Security Analysis");
            result.AppendLine($"⚠️ Could not perform schema analysis: {ex.Message}\n");
        }

        // 7. Overall Security Score
        var securityScore = CalculateSecurityScore(complexityAnalysis, depthAnalysis, introspectionRisks, injectionRisks, resourceRisks);
        result.AppendLine($"## Overall Security Score: {securityScore.Score}/100");
        result.AppendLine($"**Risk Level:** {securityScore.RiskLevel}");
        result.AppendLine($"**Recommendation:** {securityScore.Recommendation}");

        return result.ToString();
    }

    [McpServerTool, Description("Detect potential DoS attacks in GraphQL queries")]
    public static string DetectDoSPatterns(
        [Description("GraphQL query to analyze")]
        string query,
        [Description("Include detailed analysis")]
        bool includeDetails = true)
    {
        var result = new StringBuilder();
        result.AppendLine("# DoS Attack Pattern Detection\n");

        var dosPatterns = new List<DoSPattern>();

        // 1. Circular Query Detection
        var circularPatterns = DetectCircularQueries(query);
        dosPatterns.AddRange(circularPatterns);

        // 2. Resource Exhaustion Patterns
        var resourcePatterns = DetectResourceExhaustionPatterns(query);
        dosPatterns.AddRange(resourcePatterns);

        // 3. Expensive Operation Patterns
        var expensivePatterns = DetectExpensiveOperations(query);
        dosPatterns.AddRange(expensivePatterns);

        // 4. Amplification Attack Patterns
        var amplificationPatterns = DetectAmplificationAttacks(query);
        dosPatterns.AddRange(amplificationPatterns);

        // Generate report
        if (dosPatterns.Any())
        {
            result.AppendLine("⚠️ **Potential DoS patterns detected:**\n");

            foreach (var pattern in dosPatterns.OrderByDescending(p => p.Severity))
            {
                result.AppendLine($"### {pattern.Name} ({pattern.Severity} Risk)");
                result.AppendLine($"**Description:** {pattern.Description}");
                result.AppendLine($"**Impact:** {pattern.Impact}");
                result.AppendLine($"**Mitigation:** {pattern.Mitigation}\n");
            }

            result.AppendLine("## Recommendations");
            result.AppendLine("1. Implement query complexity analysis");
            result.AppendLine("2. Set query depth limits");
            result.AppendLine("3. Use query timeouts");
            result.AppendLine("4. Implement rate limiting");
            result.AppendLine("5. Consider query allowlisting for production");
        }
        else
        {
            result.AppendLine("✅ **No obvious DoS patterns detected**");
            result.AppendLine("\nThe query appears to have reasonable resource requirements.");
        }

        return result.ToString();
    }

    private static ComplexityAnalysis AnalyzeQueryComplexity(string query, int maxComplexity)
    {
        // Simplified complexity calculation
        var fieldCount = Regex.Matches(query, @"\b\w+\s*[{(]")
            .Count;
        var nestedSelections = query.Count(c => c == '{');
        var listFields = Regex.Matches(query, @"\[\s*\w+\s*\]")
            .Count;

        // Basic complexity scoring
        var score = fieldCount + (nestedSelections * 2) + (listFields * 5);

        return new ComplexityAnalysis
        {
            Score = score,
            FieldCount = fieldCount,
            NestedSelections = nestedSelections,
            ListFields = listFields
        };
    }

    private static DepthAnalysis AnalyzeQueryDepth(string query, int maxDepth)
    {
        var maxDepthFound = 0;
        var currentDepth = 0;

        foreach (var c in query)
        {
            if (c == '{')
            {
                currentDepth++;
                maxDepthFound = Math.Max(maxDepthFound, currentDepth);
            }
            else if (c == '}')
            {
                currentDepth--;
            }
        }

        return new DepthAnalysis
        {
            ActualDepth = maxDepthFound,
            ExceedsLimit = maxDepthFound > maxDepth
        };
    }

    private static List<string> DetectIntrospectionQueries(string query)
    {
        var risks = new List<string>();

        if (query.Contains("__schema") || query.Contains("__type"))
        {
            risks.Add("Schema introspection detected");
        }

        if (query.Contains("__typename"))
        {
            risks.Add("Type name introspection detected");
        }

        if (Regex.IsMatch(query, @"__\w+", RegexOptions.IgnoreCase))
        {
            risks.Add("Meta fields detected - potential information disclosure");
        }

        return risks;
    }

    private static List<string> DetectInjectionRisks(string query)
    {
        var risks = new List<string>();

        // Check for hardcoded values that should use variables
        if (Regex.IsMatch(query, @":\s*""[^""]*""") && !query.Contains("$"))
        {
            risks.Add("Hardcoded string values detected - use parameterized queries");
        }

        // Check for suspicious patterns
        if (Regex.IsMatch(query, @"['""];|--|/\*|\*/", RegexOptions.IgnoreCase))
        {
            risks.Add("SQL injection-like patterns detected");
        }

        // Check for script injection patterns
        if (Regex.IsMatch(query, @"<script|javascript:|data:", RegexOptions.IgnoreCase))
        {
            risks.Add("Script injection patterns detected");
        }

        return risks;
    }

    private static List<string> AnalyzeResourceConsumption(string query)
    {
        var risks = new List<string>();

        // Large number of fields
        var fieldCount = Regex.Matches(query, @"\b\w+\s*[{(:]")
            .Count;
        if (fieldCount > 50)
        {
            risks.Add($"High field count ({fieldCount}) may cause excessive database queries");
        }

        // Multiple aliases for same field
        var aliasMatches = Regex.Matches(query, @"(\w+):\s*(\w+)", RegexOptions.IgnoreCase);
        if (aliasMatches.Count > 10)
        {
            risks.Add($"Many field aliases ({aliasMatches.Count}) detected - potential for query multiplication");
        }

        // Nested arrays
        if (Regex.IsMatch(query, @"\[\s*\w+\s*\{.*\[\s*\w+", RegexOptions.Singleline))
        {
            risks.Add("Nested arrays detected - potential for N+1 queries");
        }

        return risks;
    }

    private static async Task<string> AnalyzeSchemaBasedSecurity(string query, string endpoint, string? headers)
    {
        try
        {
            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            var result = new StringBuilder();

            // Check for deprecated field usage
            var deprecatedFields = FindDeprecatedFieldUsage(query, schemaData);
            if (deprecatedFields.Any())
            {
                result.AppendLine("⚠️ **Deprecated field usage:**");
                foreach (var field in deprecatedFields)
                {
                    result.AppendLine($"- {field}");
                }
            }

            // Check for sensitive field patterns
            var sensitiveFields = FindSensitiveFieldPatterns(query);
            if (sensitiveFields.Any())
            {
                result.AppendLine("⚠️ **Potentially sensitive fields:**");
                foreach (var field in sensitiveFields)
                {
                    result.AppendLine($"- {field}");
                }
            }

            if (!deprecatedFields.Any() && !sensitiveFields.Any())
            {
                result.AppendLine("✅ No schema-based security issues detected");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Schema analysis failed: {ex.Message}";
        }
    }

    private static List<DoSPattern> DetectCircularQueries(string query)
    {
        var patterns = new List<DoSPattern>();

        // Simple detection of potentially recursive patterns
        var fieldNames = Regex.Matches(query, @"\b(\w+)\s*\{")
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();

        var duplicateFields = fieldNames.GroupBy(f => f)
            .Where(g => g.Count() > 2)
            .Select(g => g.Key);

        foreach (var field in duplicateFields)
        {
            patterns.Add(new DoSPattern
            {
                Name = "Potential Circular Query",
                Severity = "High",
                Description = $"Field '{field}' appears multiple times and may create circular references",
                Impact = "Could cause infinite loops and server exhaustion",
                Mitigation = "Implement query depth limits and circular reference detection"
            });
        }

        return patterns;
    }

    private static List<DoSPattern> DetectResourceExhaustionPatterns(string query)
    {
        var patterns = new List<DoSPattern>();

        // Large selection sets
        var fieldCount = Regex.Matches(query, @"\b\w+\s*[{(:]")
            .Count;
        if (fieldCount > 100)
        {
            patterns.Add(new DoSPattern
            {
                Name = "Large Selection Set",
                Severity = "Medium",
                Description = $"Query selects {fieldCount} fields",
                Impact = "High memory usage and processing time",
                Mitigation = "Implement field count limits"
            });
        }

        return patterns;
    }

    private static List<DoSPattern> DetectExpensiveOperations(string query)
    {
        var patterns = new List<DoSPattern>();

        // Search operations
        if (Regex.IsMatch(query, @"search|find|filter", RegexOptions.IgnoreCase))
        {
            patterns.Add(new DoSPattern
            {
                Name = "Search Operation",
                Severity = "Medium",
                Description = "Query contains search/filter operations",
                Impact = "Potentially expensive database operations",
                Mitigation = "Implement search rate limiting and indexing"
            });
        }

        return patterns;
    }

    private static List<DoSPattern> DetectAmplificationAttacks(string query)
    {
        var patterns = new List<DoSPattern>();

        // Multiple aliases for expensive operations
        var aliasCount = Regex.Matches(query, @"\w+:\s*\w+")
            .Count;
        if (aliasCount > 20)
        {
            patterns.Add(new DoSPattern
            {
                Name = "Query Amplification",
                Severity = "High",
                Description = $"Query uses {aliasCount} aliases",
                Impact = "Single request may trigger multiple expensive operations",
                Mitigation = "Limit alias count and implement query complexity analysis"
            });
        }

        return patterns;
    }

    private static List<string> FindDeprecatedFieldUsage(string query, JsonElement schemaData)
    {
        var deprecatedFields = new List<string>();

        // This would require parsing the schema to find deprecated fields
        // Simplified implementation for now
        if (query.Contains("@deprecated"))
        {
            deprecatedFields.Add("Query contains deprecated field markers");
        }

        return deprecatedFields;
    }

    private static List<string> FindSensitiveFieldPatterns(string query)
    {
        var sensitiveFields = new List<string>();
        var sensitivePatterns = new[]
        {
            @"\bpassword\b",
            @"\btoken\b",
            @"\bsecret\b",
            @"\bkey\b",
            @"\bcredential\b",
            @"\bsalt\b",
            @"\bhash\b"
        };

        foreach (var pattern in sensitivePatterns)
        {
            if (Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase))
            {
                sensitiveFields.Add($"Field matching pattern '{pattern}' detected");
            }
        }

        return sensitiveFields;
    }

    private static SecurityScore CalculateSecurityScore(
        ComplexityAnalysis complexity,
        DepthAnalysis depth,
        List<string> introspectionRisks,
        List<string> injectionRisks,
        List<string> resourceRisks)
    {
        var score = 100;

        // Deduct for complexity issues
        if (complexity.Score > 1000) score -= 20;
        else if (complexity.Score > 500) score -= 10;

        // Deduct for depth issues
        if (depth.ExceedsLimit) score -= 15;

        // Deduct for security risks
        score -= introspectionRisks.Count * 10;
        score -= injectionRisks.Count * 15;
        score -= resourceRisks.Count * 5;

        score = Math.Max(0, score);

        var riskLevel = score switch
        {
            >= 80 => "Low",
            >= 60 => "Medium",
            >= 40 => "High",
            _ => "Critical"
        };

        var recommendation = score switch
        {
            >= 80 => "Query appears safe for production use",
            >= 60 => "Minor security concerns - monitor usage",
            >= 40 => "Significant security risks - implement additional controls",
            _ => "Critical security issues - do not use in production"
        };

        return new SecurityScore
        {
            Score = score,
            RiskLevel = riskLevel,
            Recommendation = recommendation
        };
    }

    private class ComplexityAnalysis
    {
        public int Score { get; set; }
        public int FieldCount { get; set; }
        public int NestedSelections { get; set; }
        public int ListFields { get; set; }
    }

    private class DepthAnalysis
    {
        public int ActualDepth { get; set; }
        public bool ExceedsLimit { get; set; }
    }

    private class SecurityScore
    {
        public int Score { get; set; }
        public string RiskLevel { get; set; } = "";
        public string Recommendation { get; set; } = "";
    }

    private class DoSPattern
    {
        public string Name { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Description { get; set; } = "";
        public string Impact { get; set; } = "";
        public string Mitigation { get; set; } = "";
    }
}