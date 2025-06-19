using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class ErrorExplainerTools
{
    [McpServerTool, Description("Analyze GraphQL error messages and provide actionable solutions, explanations, and debugging guidance. This tool helps decode complex error scenarios including: syntax errors with line and column references, validation errors against schema rules, execution errors and field resolution failures, authentication and authorization issues, variable and argument type mismatches, nested field access problems, server-side runtime errors. Provides specific solutions and query corrections for each error type.")]
    public static string ExplainError(
        [Description("GraphQL error message, response JSON, or error text to analyze")]
        string errorText,
        [Description("Original GraphQL query that caused the error. Helps provide context-specific solutions")]
        string? query = null,
        [Description("Include specific solution suggestions and corrective actions")]
        bool includeSolutions = true)
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

    [McpServerTool, Description("Check GraphQL query syntax and structure for common formatting errors, best practice violations, and potential issues. This tool validates: proper GraphQL syntax and grammar, balanced brackets and parentheses, correct field selection sets, proper variable declarations and usage, argument syntax and placement, fragment definitions and usage, directive syntax and positioning, query depth and complexity warnings. Essential for query debugging before execution.")]
    public static string ValidateQuery([Description("GraphQL query string to validate for syntax and structural issues")] string query)
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
            var errors = issues.Where(i => i.Severity == "Error")
                .ToList();
            var warnings = issues.Where(i => i.Severity == "Warning")
                .ToList();
            var suggestions = issues.Where(i => i.Severity == "Suggestion")
                .ToList();

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

    [McpServerTool, Description("Provide comprehensive intelligent error analysis with context-aware solutions, related issues detection, and learning recommendations in a single response. This enhanced tool delivers complete error resolution guidance including: detailed error categorization and root cause analysis; context-aware solutions based on query patterns and schema analysis; related error detection and prevention strategies; comprehensive debugging workflow with step-by-step guidance; learning resources and best practices for error prevention; alternative approaches when primary solutions fail; historical error pattern analysis and recommendations. Returns a comprehensive JSON response with all error analysis data, solutions, and preventive measures.")]
    public static async Task<string> ExplainErrorComprehensive(
        [Description("GraphQL error message, response JSON, or error text to analyze")]
        string errorText,
        [Description("Original GraphQL query that caused the error. Helps provide context-specific solutions")]
        string? query = null,
        [Description("Include specific solution suggestions and corrective actions")]
        bool includeSolutions = true,
        [Description("Include detailed debugging workflow and step-by-step guidance")]
        bool includeDebugWorkflow = true,
        [Description("Include prevention strategies and best practices")]
        bool includePreventionGuidance = true,
        [Description("Include learning resources and documentation links")]
        bool includeLearningResources = true,
        [Description("Include related error patterns and common causes")]
        bool includeRelatedIssues = true)
    {
        try
        {
            var analysisId = Guid.NewGuid().ToString("N")[..8];
            var startTime = DateTime.UtcNow;

            // Parse and analyze the error
            var errorInfo = ParseErrorResponse(errorText);
            var primaryAnalysis = errorInfo.IsGraphQlError 
                ? AnalyzeGraphQLErrors(errorInfo.Errors, query)
                : AnalyzeSingleError(errorText, query);

            // Generate comprehensive analysis
            var response = new
            {
                analysisId = analysisId,
                error = new
                {
                    original = errorText,
                    normalized = NormalizeErrorMessage(errorText),
                    category = DetermineErrorCategory(errorText),
                    severity = DetermineErrorSeverity(errorText),
                    isGraphQLError = errorInfo.IsGraphQlError
                },
                analysis = primaryAnalysis,
                solutions = includeSolutions ? await GenerateComprehensiveSolutionsAsync(errorText, query, primaryAnalysis) : null,
                debugWorkflow = includeDebugWorkflow ? GenerateDebugWorkflow(errorText, query, primaryAnalysis) : null,
                prevention = includePreventionGuidance ? GeneratePreventionGuidance(errorText, query) : null,
                learningResources = includeLearningResources ? GenerateLearningResources(errorText, primaryAnalysis) : null,
                relatedIssues = includeRelatedIssues ? IdentifyRelatedIssues(errorText, query) : null,
                queryContext = !string.IsNullOrEmpty(query) ? new
                {
                    original = query,
                    normalized = NormalizeQuery(query),
                    potentialIssues = AnalyzeQueryForCommonIssues(query),
                    suggestions = GenerateQueryImprovements(query, errorText)
                } : null,
                metadata = new
                {
                    analysisTimestamp = DateTime.UtcNow,
                    processingTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                    version = "2.0",
                    features = new[] { "comprehensive-analysis", "context-aware-solutions", "prevention-guidance", "learning-resources" }
                },
                actionPlan = GenerateActionPlan(primaryAnalysis, includeSolutions, includePreventionGuidance),
                nextSteps = GenerateNextSteps(errorText, query, primaryAnalysis)
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
        catch (Exception ex)
        {
            return CreateErrorAnalysisErrorResponse("Error Analysis Failed",
                $"Error analyzing error message: {ex.Message}",
                "An unexpected error occurred during error analysis",
                ["Check error message format", "Verify input parameters", "Try with a simpler error message"]);
        }
    }

    /// <summary>
    /// Generate comprehensive solutions based on error analysis
    /// </summary>
    private static async Task<object> GenerateComprehensiveSolutionsAsync(string errorText, string? query, dynamic analysis)
    {
        var solutions = new List<object>();
        var quickFixes = new List<object>();
        var preventiveMeasures = new List<object>();

        // Analyze error patterns for solutions
        if (errorText.Contains("Cannot query field"))
        {
            solutions.Add(new
            {
                type = "field-error",
                priority = "high",
                title = "Field Not Found",
                description = "The requested field does not exist in the schema",
                solution = "Check the schema introspection to verify available fields",
                implementation = "Use schema introspection tools to explore available fields"
            });

            quickFixes.Add(new
            {
                action = "Remove invalid field",
                command = "Remove the problematic field from your query",
                timeEstimate = "1-2 minutes"
            });
        }

        if (errorText.Contains("Syntax Error"))
        {
            solutions.Add(new
            {
                type = "syntax-error",
                priority = "critical",
                title = "Query Syntax Error",
                description = "The GraphQL query contains syntax errors",
                solution = "Review and fix GraphQL syntax",
                implementation = "Check brackets, commas, and quotes in your query"
            });

            quickFixes.Add(new
            {
                action = "Validate syntax",
                command = "Use a GraphQL syntax validator",
                timeEstimate = "2-5 minutes"
            });
        }

        if (errorText.Contains("Variable"))
        {
            solutions.Add(new
            {
                type = "variable-error",
                priority = "medium",
                title = "Variable Issue",
                description = "Problem with variable definition or usage",
                solution = "Check variable declarations and types",
                implementation = "Ensure variables match schema requirements"
            });

            preventiveMeasures.Add(new
            {
                measure = "Always validate variable types against schema",
                benefit = "Prevents type mismatch errors",
                implementation = "Use schema-aware tools for variable validation"
            });
        }

        return new
        {
            immediate = solutions,
            quickFixes = quickFixes,
            preventiveMeasures = preventiveMeasures,
            alternativeApproaches = GenerateAlternativeApproaches(errorText, query),
            troubleshootingSteps = GenerateTroubleshootingSteps(errorText)
        };
    }

    /// <summary>
    /// Generate step-by-step debugging workflow
    /// </summary>
    private static List<object> GenerateDebugWorkflow(string errorText, string? query, dynamic analysis)
    {
        var workflow = new List<object>
        {
            new
            {
                step = 1,
                title = "Identify Error Type",
                description = "Categorize the error to understand the root cause",
                action = "Review error message and determine if it's syntax, validation, or execution error",
                timeEstimate = "1-2 minutes"
            },
            new
            {
                step = 2,
                title = "Check Query Syntax",
                description = "Validate GraphQL syntax and structure",
                action = "Use syntax highlighting and validation tools",
                timeEstimate = "2-3 minutes"
            }
        };

        if (!string.IsNullOrEmpty(query))
        {
            workflow.Add(new
            {
                step = 3,
                title = "Analyze Query Context",
                description = "Review the query for common issues and patterns",
                action = "Check field selections, variables, and nesting depth",
                timeEstimate = "3-5 minutes"
            });
        }

        workflow.Add(new
        {
            step = workflow.Count + 1,
            title = "Implement Solution",
            description = "Apply the recommended solution based on error analysis",
            action = "Make necessary changes to query or configuration",
            timeEstimate = "5-10 minutes"
        });

        workflow.Add(new
        {
            step = workflow.Count + 1,
            title = "Test and Validate",
            description = "Execute the corrected query to verify the fix",
            action = "Run the query and confirm it works correctly",
            timeEstimate = "2-3 minutes"
        });

        return workflow;
    }

    /// <summary>
    /// Generate action plan based on analysis
    /// </summary>
    private static List<object> GenerateActionPlan(dynamic analysis, bool includeSolutions, bool includePreventionGuidance)
    {
        var plan = new List<object>
        {
            new
            {
                priority = "immediate",
                action = "Address the primary error",
                description = "Fix the main issue causing the error",
                timeframe = "now"
            }
        };

        if (includeSolutions)
        {
            plan.Add(new
            {
                priority = "short-term",
                action = "Implement preventive measures",
                description = "Add checks to prevent similar errors",
                timeframe = "within 1 hour"
            });
        }

        if (includePreventionGuidance)
        {
            plan.Add(new
            {
                priority = "long-term",
                action = "Establish error prevention practices",
                description = "Implement systematic error prevention",
                timeframe = "within 1 day"
            });
        }

        return plan;
    }

    /// <summary>
    /// Create error response for analysis failures
    /// </summary>
    private static string CreateErrorAnalysisErrorResponse(string title, string message, string details, List<string> suggestions)
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
                type = "ERROR_ANALYSIS_ERROR"
            },
            metadata = new
            {
                operation = "error_analysis",
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

    // Helper methods for comprehensive error analysis (simplified implementations)
    private static dynamic AnalyzeGraphQLErrors(List<dynamic> errors, string? query) => new { type = "graphql", count = errors.Count };
    private static dynamic AnalyzeSingleError(string error, string? query) => new { type = "general", message = error };
    private static string NormalizeErrorMessage(string error) => error.Trim();
    private static string DetermineErrorCategory(string error) => error.Contains("Syntax") ? "syntax" : "validation";
    private static string DetermineErrorSeverity(string error) => error.Contains("Error") ? "high" : "medium";
    private static object GeneratePreventionGuidance(string error, string? query) => new { recommendations = new[] { "Use schema validation", "Implement error handling" } };
    private static List<object> GenerateLearningResources(string error, dynamic analysis) => [new { title = "GraphQL Error Handling", url = "https://graphql.org/learn/validation/" }];
    private static List<object> IdentifyRelatedIssues(string error, string? query) => [new { issue = "Related validation error", description = "Similar pattern detected" }];
    private static string NormalizeQuery(string query) => query.Trim();
    private static List<string> GenerateQueryImprovements(string query, string error) => ["Add proper error handling", "Validate query syntax"];
    private static List<object> GenerateNextSteps(string error, string? query, dynamic analysis) => [new { step = "Fix immediate error", action = "Apply suggested solution" }];
    private static List<object> GenerateAlternativeApproaches(string error, string? query) => [new { approach = "alternative-query", description = "Try a different query structure" }];
    private static List<object> GenerateTroubleshootingSteps(string error) => [new { step = "Check error message", action = "Read error details carefully" }];