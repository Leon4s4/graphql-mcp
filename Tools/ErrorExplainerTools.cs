using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class ErrorExplainerTools
{
    [McpServerTool, Description("Explain GraphQL errors and provide solutions")]
    public static string ExplainError(
        [Description("GraphQL error message or response")] string errorText,
        [Description("Original query that caused the error (optional)")] string? query = null,
        [Description("Include solution suggestions")] bool includeSolutions = true)
    {
       
            var explanation = new StringBuilder();
            explanation.AppendLine("# GraphQL Error Analysis\n");

            var errorInfo = ParseErrorResponse(errorText);
            
            if (errorInfo.IsGraphQlError)
            {
                explanation.AppendLine("## Error Details");
                foreach (var error in errorInfo.Errors)
                {
                    explanation.AppendLine($"### {error.Type}");
                    explanation.AppendLine($"**Message:** {error.Message}\n");
                    
                    if (!string.IsNullOrEmpty(error.Path))
                    {
                        explanation.AppendLine($"**Path:** {error.Path}");
                    }
                    
                    if (error.Locations.Any())
                    {
                        explanation.AppendLine($"**Location:** Line {error.Locations.First().Line}, Column {error.Locations.First().Column}");
                    }
                    
                    // Analyze the error type and provide explanation
                    var analysis = AnalyzeErrorType(error.Message, query);
                    explanation.AppendLine($"\n**Explanation:** {analysis.Explanation}");
                    
                    if (includeSolutions && analysis.Solutions.Any())
                    {
                        explanation.AppendLine("\n**Suggested Solutions:**");
                        foreach (var solution in analysis.Solutions)
                        {
                            explanation.AppendLine($"- {solution}");
                        }
                    }
                    
                    explanation.AppendLine();
                }
            }
            else
            {
                // Handle non-GraphQL errors
                var analysis = AnalyzeErrorType(errorText, query);
                explanation.AppendLine("## Error Analysis");
                explanation.AppendLine($"**Message:** {errorText}\n");
                explanation.AppendLine($"**Explanation:** {analysis.Explanation}\n");
                
                if (includeSolutions && analysis.Solutions.Any())
                {
                    explanation.AppendLine("**Suggested Solutions:**");
                    foreach (var solution in analysis.Solutions)
                    {
                        explanation.AppendLine($"- {solution}");
                    }
                }
            }

            // Add query context if provided
            if (!string.IsNullOrEmpty(query))
            {
                explanation.AppendLine("\n## Query Context");
                explanation.AppendLine("```graphql");
                explanation.AppendLine(query);
                explanation.AppendLine("```");
                
                var queryIssues = AnalyzeQueryForCommonIssues(query);
                if (queryIssues.Any())
                {
                    explanation.AppendLine("\n**Potential Query Issues:**");
                    foreach (var issue in queryIssues)
                    {
                        explanation.AppendLine($"- {issue}");
                    }
                }
            }

            return explanation.ToString();
    }

    [McpServerTool, Description("Validate GraphQL query syntax and structure")]
    public static string ValidateQuery([Description("GraphQL query to validate")] string query)
    {
            var validation = new StringBuilder();
            validation.AppendLine("# GraphQL Query Validation Report\n");

            var issues = new List<ValidationIssue>();
            
            // Basic syntax validation
            issues.AddRange(ValidateSyntax(query));
            
            // Structure validation
            issues.AddRange(ValidateStructure(query));
            
            // Best practices validation
            issues.AddRange(ValidateBestPractices(query));

            if (!issues.Any())
            {
                validation.AppendLine("✅ **Query is valid!**\n");
                validation.AppendLine("No syntax or structural issues found.");
            }
            else
            {
                var errors = issues.Where(i => i.Severity == "Error").ToList();
                var warnings = issues.Where(i => i.Severity == "Warning").ToList();
                var suggestions = issues.Where(i => i.Severity == "Suggestion").ToList();

                if (errors.Any())
                {
                    validation.AppendLine("## ❌ Errors");
                    foreach (var error in errors)
                    {
                        validation.AppendLine($"- **{error.Message}**");
                        if (!string.IsNullOrEmpty(error.Location))
                        {
                            validation.AppendLine($"  Location: {error.Location}");
                        }
                        if (!string.IsNullOrEmpty(error.Fix))
                        {
                            validation.AppendLine($"  Fix: {error.Fix}");
                        }
                        validation.AppendLine();
                    }
                }

                if (warnings.Any())
                {
                    validation.AppendLine("## ⚠️ Warnings");
                    foreach (var warning in warnings)
                    {
                        validation.AppendLine($"- **{warning.Message}**");
                        if (!string.IsNullOrEmpty(warning.Location))
                        {
                            validation.AppendLine($"  Location: {warning.Location}");
                        }
                        if (!string.IsNullOrEmpty(warning.Fix))
                        {
                            validation.AppendLine($"  Fix: {warning.Fix}");
                        }
                        validation.AppendLine();
                    }
                }

                if (suggestions.Any())
                {
                    validation.AppendLine("## 💡 Suggestions");
                    foreach (var suggestion in suggestions)
                    {
                        validation.AppendLine($"- **{suggestion.Message}**");
                        if (!string.IsNullOrEmpty(suggestion.Fix))
                        {
                            validation.AppendLine($"  Suggestion: {suggestion.Fix}");
                        }
                        validation.AppendLine();
                    }
                }
            }

            return validation.ToString();
      
    }

    private static ErrorResponse ParseErrorResponse(string errorText)
    {
        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(errorText);
            
            if (jsonElement.TryGetProperty("errors", out var errorsElement))
            {
                var errors = new List<GraphQlError>();
                
                foreach (var error in errorsElement.EnumerateArray())
                {
                    var graphqlError = new GraphQlError
                    {
                        Message = error.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "",
                        Type = DetermineErrorType(error.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "")
                    };
                    
                    if (error.TryGetProperty("path", out var pathElement))
                    {
                        graphqlError.Path = string.Join(".", pathElement.EnumerateArray().Select(p => p.ToString()));
                    }
                    
                    if (error.TryGetProperty("locations", out var locationsElement))
                    {
                        foreach (var location in locationsElement.EnumerateArray())
                        {
                            if (location.TryGetProperty("line", out var line) && 
                                location.TryGetProperty("column", out var column))
                            {
                                graphqlError.Locations.Add(new ErrorLocation
                                {
                                    Line = line.GetInt32(),
                                    Column = column.GetInt32()
                                });
                            }
                        }
                    }
                    
                    errors.Add(graphqlError);
                }
                
                return new ErrorResponse { IsGraphQlError = true, Errors = errors };
            }
        }
        catch
        {
            // Not a JSON response, treat as plain text error
        }
        
        return new ErrorResponse 
        { 
            IsGraphQlError = false, 
            Errors =
            [
                new()
                {
                    Message = errorText,
                    Type = DetermineErrorType(errorText)
                }
            ]
        };
    }

    private static string DetermineErrorType(string message)
    {
        var lowerMessage = message.ToLower();
        
        if (lowerMessage.Contains("syntax") || lowerMessage.Contains("unexpected"))
            return "Syntax Error";
        if (lowerMessage.Contains("validation") || lowerMessage.Contains("invalid"))
            return "Validation Error";
        if (lowerMessage.Contains("field") && lowerMessage.Contains("exist"))
            return "Field Error";
        if (lowerMessage.Contains("type") && lowerMessage.Contains("exist"))
            return "Type Error";
        if (lowerMessage.Contains("argument"))
            return "Argument Error";
        if (lowerMessage.Contains("permission") || lowerMessage.Contains("unauthorized"))
            return "Authorization Error";
        if (lowerMessage.Contains("network") || lowerMessage.Contains("connection"))
            return "Network Error";
        
        return "General Error";
    }

    private static ErrorAnalysis AnalyzeErrorType(string message, string? query)
    {
        var lowerMessage = message.ToLower();
        var analysis = new ErrorAnalysis();
        
        if (lowerMessage.Contains("syntax error") || lowerMessage.Contains("unexpected"))
        {
            analysis.Explanation = "This is a syntax error, meaning the GraphQL query is not properly formatted according to GraphQL syntax rules.";
            analysis.Solutions.Add("Check for missing braces, parentheses, or commas");
            analysis.Solutions.Add("Ensure field names and types are correctly spelled");
            analysis.Solutions.Add("Verify that the query follows GraphQL syntax conventions");
        }
        else if (lowerMessage.Contains("field") && lowerMessage.Contains("exist"))
        {
            analysis.Explanation = "The query is trying to access a field that doesn't exist on the specified type.";
            analysis.Solutions.Add("Check the schema to see available fields for this type");
            analysis.Solutions.Add("Verify the field name spelling");
            analysis.Solutions.Add("Ensure you're querying the correct type");
        }
        else if (lowerMessage.Contains("argument"))
        {
            analysis.Explanation = "There's an issue with the arguments provided to a field.";
            analysis.Solutions.Add("Check if required arguments are missing");
            analysis.Solutions.Add("Verify argument types match the schema definition");
            analysis.Solutions.Add("Ensure argument names are correctly spelled");
        }
        else if (lowerMessage.Contains("type"))
        {
            analysis.Explanation = "There's an issue with type usage in the query.";
            analysis.Solutions.Add("Verify the type exists in the schema");
            analysis.Solutions.Add("Check for correct type usage in fragments or variables");
            analysis.Solutions.Add("Ensure interface implementations are correct");
        }
        else if (lowerMessage.Contains("permission") || lowerMessage.Contains("unauthorized"))
        {
            analysis.Explanation = "The query failed due to insufficient permissions or authentication issues.";
            analysis.Solutions.Add("Check if you're properly authenticated");
            analysis.Solutions.Add("Verify you have permission to access the requested data");
            analysis.Solutions.Add("Review the authorization headers in your request");
        }
        else
        {
            analysis.Explanation = "This appears to be a general GraphQL error that needs further investigation.";
            analysis.Solutions.Add("Check the server logs for more detailed error information");
            analysis.Solutions.Add("Verify the GraphQL endpoint is accessible and working");
            analysis.Solutions.Add("Test with a simpler query to isolate the issue");
        }
        
        return analysis;
    }

    private static List<string> AnalyzeQueryForCommonIssues(string query)
    {
        var issues = new List<string>();
        
        // Check for unmatched braces
        var braceCount = 0;
        foreach (var c in query)
        {
            if (c == '{') braceCount++;
            else if (c == '}') braceCount--;
        }
        if (braceCount != 0)
        {
            issues.Add("Unmatched braces detected in query");
        }
        
        // Check for common syntax issues
        if (Regex.IsMatch(query, @"{\s*}", RegexOptions.IgnoreCase))
        {
            issues.Add("Empty selection sets found");
        }
        
        if (Regex.IsMatch(query, @"\w+\s+\w+(?!\s*[:(])", RegexOptions.IgnoreCase))
        {
            issues.Add("Possible missing field separator (missing comma or newline)");
        }
        
        return issues;
    }

    private static List<ValidationIssue> ValidateSyntax(string query)
    {
        var issues = new List<ValidationIssue>();
        
        // Check for balanced braces
        var braceCount = 0;
        var line = 1;
        var column = 1;
        
        foreach (var c in query)
        {
            if (c == '{')
            {
                braceCount++;
            }
            else if (c == '}')
            {
                braceCount--;
                if (braceCount < 0)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = "Error",
                        Message = "Unexpected closing brace",
                        Location = $"Line {line}, Column {column}",
                        Fix = "Remove the extra closing brace or add a matching opening brace"
                    });
                }
            }
            else if (c == '\n')
            {
                line++;
                column = 1;
                continue;
            }
            
            column++;
        }
        
        if (braceCount > 0)
        {
            issues.Add(new ValidationIssue
            {
                Severity = "Error",
                Message = "Unclosed selection set",
                Location = "End of query",
                Fix = "Add missing closing braces"
            });
        }
        
        return issues;
    }

    private static List<ValidationIssue> ValidateStructure(string query)
    {
        var issues = new List<ValidationIssue>();
        
        // Check for empty selection sets
        if (Regex.IsMatch(query, @"{\s*}", RegexOptions.IgnoreCase))
        {
            issues.Add(new ValidationIssue
            {
                Severity = "Error",
                Message = "Empty selection set found",
                Fix = "Add fields to the selection set or remove it"
            });
        }
        
        // Check for duplicate fields
        var fieldMatches = Regex.Matches(query, @"\b(\w+)(?=\s*[{(]|\s*$)", RegexOptions.IgnoreCase);
        var fieldCounts = new Dictionary<string, int>();
        
        foreach (Match match in fieldMatches)
        {
            var field = match.Groups[1].Value;
            fieldCounts[field] = fieldCounts.GetValueOrDefault(field, 0) + 1;
        }
        
        foreach (var field in fieldCounts.Where(f => f.Value > 1))
        {
            issues.Add(new ValidationIssue
            {
                Severity = "Warning",
                Message = $"Field '{field.Key}' appears multiple times",
                Fix = "Consider using aliases for duplicate fields or combine them"
            });
        }
        
        return issues;
    }

    private static List<ValidationIssue> ValidateBestPractices(string query)
    {
        var issues = new List<ValidationIssue>();
        
        // Check for deep nesting
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
        
        if (maxDepth > 10)
        {
            issues.Add(new ValidationIssue
            {
                Severity = "Warning",
                Message = $"Query has deep nesting ({maxDepth} levels)",
                Fix = "Consider breaking the query into smaller parts or using fragments"
            });
        }
        
        // Check for unnamed operations
        if (!Regex.IsMatch(query, @"^\s*(query|mutation|subscription)\s+\w+", RegexOptions.IgnoreCase) && 
            !query.TrimStart().StartsWith("{"))
        {
            issues.Add(new ValidationIssue
            {
                Severity = "Suggestion",
                Message = "Consider naming your operations",
                Fix = "Add operation names like 'query GetUser' for better debugging and caching"
            });
        }
        
        return issues;
    }

    private class ErrorResponse
    {
        public bool IsGraphQlError { get; set; }
        public List<GraphQlError> Errors { get; set; } = [];
    }

    private class GraphQlError
    {
        public string Message { get; set; } = "";
        public string Type { get; set; } = "";
        public string Path { get; set; } = "";
        public List<ErrorLocation> Locations { get; set; } = [];
    }

    private class ErrorLocation
    {
        public int Line { get; set; }
        public int Column { get; set; }
    }

    private class ErrorAnalysis
    {
        public string Explanation { get; set; } = "";
        public List<string> Solutions { get; set; } = [];
    }

    private class ValidationIssue
    {
        public string Severity { get; set; } = "";
        public string Message { get; set; } = "";
        public string Location { get; set; } = "";
        public string Fix { get; set; } = "";
    }
}
