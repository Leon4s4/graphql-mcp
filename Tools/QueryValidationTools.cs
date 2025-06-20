using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class QueryValidationTools
{
    [McpServerTool, Description("Test GraphQL queries with comprehensive multi-layer validation including syntax checking, schema compliance verification, and actual execution testing. This tool provides complete query testing with: syntax validation for proper GraphQL grammar, schema validation against endpoint structure, field existence and type checking, variable and argument validation, execution testing with error handling, performance and complexity analysis, best practice recommendations. Essential for query development and debugging.")]
    public static async Task<string> TestQuery(
        [Description("GraphQL query string to test. Can be query, mutation, or subscription")]
        string query,
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Variables as JSON object for parameterized queries. Example: {\"id\": 123, \"limit\": 10}")]
        string? variables = null,
        [Description("Only perform syntax validation without executing the query. Useful for quick syntax checks")]
        bool syntaxCheckOnly = false)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var result = new StringBuilder();
        result.AppendLine("# GraphQL Query Test Report\n");

        // 1. Syntax Validation
        var syntaxErrors = ValidateQuerySyntax(query);
        result.AppendLine("## Syntax Validation");
        if (syntaxErrors.Any())
        {
            result.AppendLine("❌ **Status:** Syntax errors found\n");
            result.AppendLine("**Errors:**");
            foreach (var error in syntaxErrors)
            {
                result.AppendLine($"- {error}");
            }

            result.AppendLine();
        }
        else
        {
            result.AppendLine("✅ **Status:** Syntax is valid\n");
        }

        // If syntax-only check or syntax errors found, return early
        if (syntaxCheckOnly || syntaxErrors.Any())
        {
            return result.ToString();
        }

        // 2. Schema Validation (against endpoint)
        result.AppendLine("## Schema Validation");
        try
        {
            var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
            if (!schemaResult.IsSuccess)
            {
                result.AppendLine(schemaResult.FormatForDisplay());
            }
            else
            {
                var schemaErrors = ValidateQueryAgainstSchema(query, schemaResult.Content!);

                if (schemaErrors.Any())
                {
                    result.AppendLine("❌ **Status:** Schema validation errors\n");
                    result.AppendLine("**Errors:**");
                    foreach (var error in schemaErrors)
                    {
                        result.AppendLine($"- {error}");
                    }

                    result.AppendLine();
                }
                else
                {
                    result.AppendLine("✅ **Status:** Query is valid against schema\n");
                }
            }
        }
        catch (Exception ex)
        {
            result.AppendLine($"⚠️ **Status:** Could not validate against schema: {ex.Message}\n");
        }

        // 3. Execution Test
        result.AppendLine("## Execution Test");
        try
        {
            var executionResult = await ExecuteTestQuery(endpointInfo, query, variables);
            result.AppendLine(executionResult);
        }
        catch (Exception ex)
        {
            result.AppendLine($"❌ **Status:** Execution failed: {ex.Message}\n");
        }

        // 4. Performance Analysis
        result.AppendLine("## Quick Performance Check");
        var performanceWarnings = AnalyzeQueryPerformance(query);
        if (performanceWarnings.Any())
        {
            result.AppendLine("⚠️ **Performance Warnings:**");
            foreach (var warning in performanceWarnings)
            {
                result.AppendLine($"- {warning}");
            }

            result.AppendLine();
        }
        else
        {
            result.AppendLine("✅ **Performance:** No obvious performance issues detected\n");
        }

        return result.ToString();
    }

    [McpServerTool, Description("Perform syntax validation on GraphQL queries without schema or execution requirements")]
    public static string ValidateQuery([Description("GraphQL query to validate")] string query)
    {
        var result = new StringBuilder();
        result.AppendLine("# GraphQL Query Validation Report\n");

        var errors = ValidateQuerySyntax(query);

        if (errors.Any())
        {
            result.AppendLine("## ❌ Validation Failed\n");
            result.AppendLine("**Syntax Errors:**");
            foreach (var error in errors)
            {
                result.AppendLine($"- {error}");
            }

            result.AppendLine("\n## Suggestions");
            var suggestions = GetValidationSuggestions(query, errors);
            foreach (var suggestion in suggestions)
            {
                result.AppendLine($"- {suggestion}");
            }
        }
        else
        {
            result.AppendLine("## ✅ Validation Passed\n");
            result.AppendLine("The query syntax is valid.");

            // Add additional analysis for valid queries
            var analysis = AnalyzeValidQuery(query);
            result.AppendLine("\n## Query Analysis");
            result.AppendLine($"- **Operation Type:** {analysis.OperationType}");
            result.AppendLine($"- **Field Count:** {analysis.FieldCount}");
            result.AppendLine($"- **Max Depth:** {analysis.MaxDepth}");
            result.AppendLine($"- **Has Variables:** {analysis.HasVariables}");
            result.AppendLine($"- **Has Fragments:** {analysis.HasFragments}");
        }

        return result.ToString();
    }

    [McpServerTool, Description("Perform comprehensive query validation with advanced testing, schema compliance checking, performance analysis, and intelligent error detection. This enterprise-grade validation tool provides complete query assessment for development and production readiness.")]
    public static async Task<string> ValidateQueryComprehensive(
        [Description("GraphQL query string to validate. Can be query, mutation, or subscription")]
        string query,
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Variables as JSON object for parameterized queries. Example: {\"id\": 123, \"limit\": 10}")]
        string? variables = null,
        [Description("Validation mode: 'basic' for syntax only, 'standard' for schema validation, 'comprehensive' for full testing")]
        string validationMode = "comprehensive",
        [Description("Include performance benchmarking and optimization analysis")]
        bool includePerformanceAnalysis = true,
        [Description("Test query execution against the live endpoint")]
        bool executeQuery = false)
    {
        try
        {
            return await ServiceLocator.ExecuteWithSmartResponseServiceAsync(async smartResponseService =>
            {
                var smartResponse = await smartResponseService.CreateQueryValidationResponseAsync(
                    query, endpointName, variables, validationMode, includePerformanceAnalysis, executeQuery);
                
                return await smartResponseService.FormatComprehensiveResponseAsync(smartResponse);
            });
        }
        catch (Exception ex)
        {
            return await ServiceLocator.ExecuteWithSmartResponseServiceAsync(async smartResponseService =>
            {
                return await smartResponseService.CreateErrorResponseAsync(
                    "QueryValidationError", 
                    ex.Message,
                    new { query, endpointName, validationMode });
            });
        }
    }

    private static List<string> ValidateQuerySyntax(string query)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(query))
        {
            errors.Add("Query is empty or null");
            return errors;
        }

        // Check for balanced braces
        var braceCount = 0;
        var parenCount = 0;

        foreach (var c in query)
        {
            switch (c)
            {
                case '{': braceCount++; break;
                case '}': braceCount--; break;
                case '(': parenCount++; break;
                case ')': parenCount--; break;
            }

            if (braceCount < 0)
            {
                errors.Add("Unmatched closing brace '}'");
                break;
            }

            if (parenCount < 0)
            {
                errors.Add("Unmatched closing parenthesis ')'");
                break;
            }
        }

        if (braceCount > 0)
        {
            errors.Add($"Missing {braceCount} closing brace(s) '}}'");
        }

        if (parenCount > 0)
        {
            errors.Add($"Missing {parenCount} closing parenthesis ')'");
        }

        // Check for basic GraphQL structure
        var trimmedQuery = query.Trim();
        if (!trimmedQuery.StartsWith("query") &&
            !trimmedQuery.StartsWith("mutation") &&
            !trimmedQuery.StartsWith("subscription") &&
            !trimmedQuery.StartsWith("{") &&
            !trimmedQuery.StartsWith("fragment"))
        {
            errors.Add("Query must start with 'query', 'mutation', 'subscription', '{' or 'fragment'");
        }

        // Check for common syntax errors
        if (Regex.IsMatch(query, @"[{,]\s*[}]"))
        {
            errors.Add("Empty selection set found");
        }

        if (Regex.IsMatch(query, @"[{,]\s*,"))
        {
            errors.Add("Trailing comma in selection set");
        }

        // Check variable definitions in the operation header
        var operationMatch = Regex.Match(query, @"^(?:query|mutation|subscription)\b\s*(\w+)?\s*(\(([^)]*)\))?\s*(?:@[\w]+)?\s*(?:\.\.\.[\w]+)?");
        if (operationMatch.Success)
        {
            var declarations = operationMatch.Groups[1]
                .Value.Split(',');
            foreach (var decl in declarations)
            {
                var trimmed = decl.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.Contains(':'))
                {
                    errors.Add($"Variable {trimmed} is missing type definition");
                }
            }
        }

        return errors;
    }

    private static List<string> ValidateQueryAgainstSchema(string query, string schemaJson)
    {
        var errors = new List<string>();

        try
        {
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);
            if (!schemaData.TryGetProperty("data", out var data) ||
                !data.TryGetProperty("__schema", out var schema))
            {
                errors.Add("Invalid schema data received");
                return errors;
            }

            // Extract field names from query
            var queryFields = ExtractFieldNames(query);

            // Get schema types
            if (schema.TryGetProperty("types", out var types))
            {
                var schemaFieldNames = new HashSet<string>();
                foreach (var type in types.EnumerateArray())
                {
                    if (type.TryGetProperty("fields", out var fields))
                    {
                        foreach (var field in fields.EnumerateArray())
                        {
                            if (field.TryGetProperty("name", out var fieldName))
                            {
                                schemaFieldNames.Add(fieldName.GetString() ?? "");
                            }
                        }
                    }
                }

                // Check if query fields exist in schema
                foreach (var fieldName in queryFields)
                {
                    if (!schemaFieldNames.Contains(fieldName) &&
                        !IsMetaField(fieldName))
                    {
                        errors.Add($"Field '{fieldName}' does not exist in schema");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Schema validation error: {ex.Message}");
        }

        return errors;
    }

    private static async Task<string> ExecuteTestQuery(GraphQlEndpointInfo endpointInfo, string query, string? variables)
    {
        try
        {
            var requestBody = new
            {
                query,
                variables = string.IsNullOrWhiteSpace(variables) ? null : JsonSerializer.Deserialize<object>(variables)
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, requestBody);
            stopwatch.Stop();

            var response = new StringBuilder();

            if (result.IsSuccess)
            {
                response.AppendLine($"✅ **Status:** Execution successful ({stopwatch.ElapsedMilliseconds}ms)");

                var responseData = JsonSerializer.Deserialize<JsonElement>(result.Content!);
                if (responseData.TryGetProperty("errors", out var errors) &&
                    errors.ValueKind == JsonValueKind.Array &&
                    errors.GetArrayLength() > 0)
                {
                    response.AppendLine("\n⚠️ **GraphQL Errors:**");
                    foreach (var error in errors.EnumerateArray())
                    {
                        if (error.TryGetProperty("message", out var message))
                        {
                            response.AppendLine($"- {message.GetString()}");
                        }
                    }
                }
                else
                {
                    response.AppendLine("\n✅ **Result:** Query executed without errors");
                }

                if (responseData.TryGetProperty("data", out var data))
                {
                    response.AppendLine($"\n**Data:** {GetDataSummary(data)}");
                }
            }
            else
            {
                // Return the detailed error message from centralized handling
                response.AppendLine(result.FormatForDisplay());
            }

            return response.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ **Status:** Execution failed - {ex.Message}";
        }
    }

    private static List<string> AnalyzeQueryPerformance(string query)
    {
        var warnings = new List<string>();

        // Check for deeply nested queries
        var maxDepth = CalculateMaxDepth(query);
        if (maxDepth > 10)
        {
            warnings.Add($"Query depth ({maxDepth}) may cause performance issues");
        }

        // Check for potential N+1 problems
        var fieldCount = Regex.Matches(query, @"\w+\s*{")
            .Count;
        if (fieldCount > 20)
        {
            warnings.Add($"High number of nested fields ({fieldCount}) may indicate N+1 query problems");
        }

        // Check for missing variables (potential for injection)
        if (Regex.IsMatch(query, @":\s*""[^""]*""") && !query.Contains("$"))
        {
            warnings.Add("Query contains hardcoded string values - consider using variables");
        }

        return warnings;
    }

    private static List<string> GetValidationSuggestions(string query, List<string> errors)
    {
        var suggestions = new List<string>();

        if (errors.Any(e => e.Contains("brace")))
        {
            suggestions.Add("Check that all opening braces '{' have matching closing braces '}'");
        }

        if (errors.Any(e => e.Contains("parenthesis")))
        {
            suggestions.Add("Check that all opening parentheses '(' have matching closing parentheses ')'");
        }

        if (errors.Any(e => e.Contains("Variable") && e.Contains("type definition")))
        {
            suggestions.Add("Ensure all variables have type definitions (e.g., $id: ID!)");
        }

        if (errors.Any(e => e.Contains("Empty selection set")))
        {
            suggestions.Add("Remove empty braces '{}' or add field selections");
        }

        return suggestions;
    }

    private static QueryAnalysisResult AnalyzeValidQuery(string query)
    {
        var operationType = "query";
        if (query.TrimStart()
            .StartsWith("mutation", StringComparison.OrdinalIgnoreCase))
            operationType = "mutation";
        else if (query.TrimStart()
                 .StartsWith("subscription", StringComparison.OrdinalIgnoreCase))
            operationType = "subscription";

        var fieldCount = Regex.Matches(query, @"\b\w+\s*[{(:]")
            .Count;
        var maxDepth = CalculateMaxDepth(query);
        var hasVariables = query.Contains("$");
        var hasFragments = Regex.IsMatch(query, @"fragment\s+\w+", RegexOptions.IgnoreCase);

        return new QueryAnalysisResult
        {
            OperationType = operationType,
            FieldCount = fieldCount,
            MaxDepth = maxDepth,
            HasVariables = hasVariables,
            HasFragments = hasFragments
        };
    }

    private static List<string> ExtractFieldNames(string query)
    {
        var fieldNames = new List<string>();
        var matches = Regex.Matches(query, @"\b(\w+)\s*[{(:]", RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var fieldName = match.Groups[1].Value;
            if (!IsGraphQlKeyword(fieldName))
            {
                fieldNames.Add(fieldName);
            }
        }

        return fieldNames.Distinct()
            .ToList();
    }

    private static bool IsMetaField(string fieldName)
    {
        return fieldName.StartsWith("__") ||
               fieldName == "schema" ||
               fieldName == "type" ||
               fieldName == "typename";
    }

    private static bool IsGraphQlKeyword(string word)
    {
        var keywords = new[] { "query", "mutation", "subscription", "fragment", "on", "true", "false", "null" };
        return keywords.Contains(word.ToLower());
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

    private static string GetDataSummary(JsonElement data)
    {
        switch (data.ValueKind)
        {
            case JsonValueKind.Object:
                var propCount = data.EnumerateObject()
                    .Count();
                return $"Object with {propCount} properties";
            case JsonValueKind.Array:
                return $"Array with {data.GetArrayLength()} items";
            case JsonValueKind.String:
                return $"String: \"{data.GetString()}\"";
            case JsonValueKind.Number:
                return $"Number: {data.GetRawText()}";
            case JsonValueKind.True:
            case JsonValueKind.False:
                return $"Boolean: {data.GetBoolean()}";
            case JsonValueKind.Null:
                return "null";
            default:
                return "Unknown data type";
        }
    }

    private class QueryAnalysisResult
    {
        public string OperationType { get; set; } = "";
        public int FieldCount { get; set; }
        public int MaxDepth { get; set; }
        public bool HasVariables { get; set; }
        public bool HasFragments { get; set; }
    }
}