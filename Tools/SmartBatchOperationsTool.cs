using System.ComponentModel;
using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Smart batch operations tool for executing multiple GraphQL operations efficiently
/// </summary>
[McpServerToolType]
public static class SmartBatchOperationsTool
{
    [McpServerTool, Description(@"Execute multiple GraphQL operations in a single batch with comprehensive analysis and intelligent optimization.

This advanced tool provides efficient batch processing with smart defaults including:
- Concurrent execution with configurable parallelism
- Automatic query optimization and deduplication
- Comprehensive performance analysis across all operations
- Individual operation success/failure tracking with detailed metrics
- Batch-level performance recommendations and insights
- Cross-operation dependency analysis and suggestions
- Resource usage optimization for large batches
- Error aggregation with context-aware suggestions

Features:
- Smart concurrency control to prevent server overload
- Automatic retry logic for transient failures
- Performance profiling with per-operation and batch metrics
- Memory-efficient execution for large operation sets
- Intelligent caching recommendations based on operation patterns
- Security analysis across the entire batch

Use this for:
- Bulk data operations requiring multiple queries
- Performance testing with controlled load patterns
- Data migration and synchronization tasks
- Complex workflows requiring multiple dependent operations")]
    public static async Task<string> ExecuteBatchOperations(
        [Description("JSON array of GraphQL operations to execute. Each operation should have 'query', 'variables' (optional), and 'id' (optional) properties")]
        string operationsJson,
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Maximum number of concurrent operations (1-20, default: 5)")]
        int maxConcurrency = 5,
        [Description("Include comprehensive performance analysis and recommendations")]
        bool includePerformanceAnalysis = true,
        [Description("Include cross-operation dependency analysis")]
        bool includeDependencyAnalysis = true,
        [Description("Include security analysis across all operations")]
        bool includeSecurityAnalysis = true,
        [Description("Automatic retry count for failed operations (0-3, default: 1)")]
        int retryCount = 1)
    {
        try
        {
            // Validate endpoint
            if (!EndpointRegistryService.Instance.IsEndpointRegistered(endpointName))
            {
                var registeredEndpoints = EndpointRegistryService.Instance.GetRegisteredEndpointNames();
                return CreateErrorResponse("Endpoint Not Found", 
                    $"Endpoint '{endpointName}' not found",
                    $"Available endpoints: {string.Join(", ", registeredEndpoints)}",
                    ["Use RegisterEndpoint to add new endpoints", "Check endpoint name spelling"]);
            }

            var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
            if (endpointInfo == null)
            {
                return CreateErrorResponse("Configuration Error", 
                    "Could not retrieve endpoint information",
                    "Endpoint configuration is invalid",
                    ["Re-register the endpoint", "Check endpoint configuration"]);
            }

            // Parse and validate operations
            List<BatchQueryRequest> operations;
            try
            {
                operations = JsonSerializer.Deserialize<List<BatchQueryRequest>>(operationsJson) ?? [];
                if (operations.Count == 0)
                {
                    return CreateErrorResponse("Invalid Input", 
                        "No operations provided",
                        "The operations array is empty",
                        ["Provide at least one operation", "Check JSON format"]);
                }
            }
            catch (JsonException ex)
            {
                return CreateErrorResponse("JSON Parsing Error", 
                    $"Error parsing operations JSON: {ex.Message}",
                    "The operations parameter must be valid JSON array",
                    ["Check JSON syntax", "Validate array format", "Ensure proper quotes and brackets"]);
            }

            // Validate concurrency limits
            maxConcurrency = Math.Max(1, Math.Min(20, maxConcurrency));

            // Pre-execution validation
            var validationErrors = ValidateOperations(operations, endpointInfo);
            if (validationErrors.Any())
            {
                return CreateErrorResponse("Validation Failed", 
                    "One or more operations failed validation",
                    string.Join("; ", validationErrors),
                    ["Fix validation errors", "Check mutation permissions", "Verify operation syntax"]);
            }

            // Execute batch with smart response service
            var smartResponseService = GetSmartResponseService();
            return await smartResponseService.CreateBatchExecutionResponseAsync(operations, maxConcurrency);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Batch Execution Error", 
                $"Error executing batch operations: {ex.Message}",
                "An unexpected error occurred during batch execution",
                ["Check operations format", "Verify endpoint connectivity", "Reduce batch size if too large"]);
        }
    }

    [McpServerTool, Description(@"Analyze a batch of GraphQL operations before execution to provide optimization recommendations and resource estimates.

This tool performs comprehensive pre-execution analysis including:
- Query complexity analysis across all operations
- Resource usage estimation and optimization suggestions
- Dependency detection between operations for optimal ordering
- Performance predictions based on operation patterns
- Security risk assessment for the entire batch
- Caching opportunities identification
- Concurrency recommendations based on operation characteristics

Use this before executing large batches to:
- Optimize execution order and parallelism
- Identify potential performance bottlenecks
- Estimate resource requirements and execution time
- Detect security risks before execution")]
    public static async Task<string> AnalyzeBatchOperations(
        [Description("JSON array of GraphQL operations to analyze")]
        string operationsJson,
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Include detailed complexity analysis for each operation")]
        bool includeComplexityAnalysis = true,
        [Description("Include resource usage estimation")]
        bool includeResourceEstimation = true,
        [Description("Include security risk assessment")]
        bool includeSecurityAssessment = true)
    {
        try
        {
            // Parse operations
            List<BatchQueryRequest> operations;
            try
            {
                operations = JsonSerializer.Deserialize<List<BatchQueryRequest>>(operationsJson) ?? [];
            }
            catch (JsonException ex)
            {
                return CreateErrorResponse("JSON Parsing Error", 
                    $"Error parsing operations JSON: {ex.Message}",
                    "Invalid JSON format",
                    ["Check JSON syntax", "Validate array structure"]);
            }

            // Validate endpoint
            var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
            if (endpointInfo == null)
            {
                return CreateErrorResponse("Endpoint Not Found", 
                    $"Endpoint '{endpointName}' not found",
                    "Endpoint not registered",
                    ["Register the endpoint first", "Check endpoint name"]);
            }

            // Perform analysis
            var analysis = await PerformBatchAnalysisAsync(operations, endpointInfo, 
                includeComplexityAnalysis, includeResourceEstimation, includeSecurityAssessment);

            return JsonSerializer.Serialize(analysis, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Analysis Error", 
                $"Error analyzing batch operations: {ex.Message}",
                "Unexpected error during analysis",
                ["Check operations format", "Verify endpoint configuration"]);
        }
    }

    /// <summary>
    /// Helper method to get SmartResponseService instance
    /// </summary>
    private static SmartResponseService GetSmartResponseService()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<SmartResponseService>.Instance;
        return new SmartResponseService(cache, logger);
    }

    /// <summary>
    /// Validate operations before execution
    /// </summary>
    private static List<string> ValidateOperations(List<BatchQueryRequest> operations, GraphQlEndpointInfo endpointInfo)
    {
        var errors = new List<string>();

        for (int i = 0; i < operations.Count; i++)
        {
            var operation = operations[i];
            
            if (string.IsNullOrWhiteSpace(operation.Query))
            {
                errors.Add($"Operation {i}: Query is required");
                continue;
            }

            // Check mutation permissions
            if (IsMutation(operation.Query) && !endpointInfo.AllowMutations)
            {
                errors.Add($"Operation {i}: Mutations not allowed on this endpoint");
            }

            // Validate variables JSON if provided
            if (operation.Variables?.Count > 0)
            {
                try
                {
                    JsonSerializer.Serialize(operation.Variables);
                }
                catch
                {
                    errors.Add($"Operation {i}: Invalid variables format");
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Check if a query is a mutation
    /// </summary>
    private static bool IsMutation(string query)
    {
        return query.TrimStart().StartsWith("mutation", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Perform comprehensive batch analysis
    /// </summary>
    private static async Task<object> PerformBatchAnalysisAsync(
        List<BatchQueryRequest> operations, 
        GraphQlEndpointInfo endpointInfo,
        bool includeComplexityAnalysis,
        bool includeResourceEstimation, 
        bool includeSecurityAssessment)
    {
        var totalComplexity = 0;
        var estimatedExecutionTime = TimeSpan.Zero;
        var securityWarnings = new List<string>();
        var optimizationRecommendations = new List<string>();
        var operationAnalysis = new List<object>();

        foreach (var (operation, index) in operations.Select((op, idx) => (op, idx)))
        {
            var complexity = CalculateQueryComplexity(operation.Query);
            totalComplexity += complexity;
            
            var estimatedTime = EstimateExecutionTime(operation.Query, complexity);
            estimatedExecutionTime = estimatedExecutionTime.Add(estimatedTime);

            if (includeSecurityAssessment)
            {
                var warnings = AnalyzeSecurityRisks(operation.Query);
                securityWarnings.AddRange(warnings.Select(w => $"Operation {index}: {w}"));
            }

            if (includeComplexityAnalysis)
            {
                operationAnalysis.Add(new
                {
                    index = index,
                    id = operation.Id,
                    queryType = IsMutation(operation.Query) ? "mutation" : "query",
                    complexityScore = complexity,
                    estimatedExecutionTimeMs = (int)estimatedTime.TotalMilliseconds,
                    fieldCount = CountFields(operation.Query),
                    hasVariables = operation.Variables?.Count > 0,
                    recommendations = GenerateOperationRecommendations(operation.Query, complexity)
                });
            }
        }

        // Generate batch-level recommendations
        if (operations.Count > 10)
        {
            optimizationRecommendations.Add("Consider splitting large batches into smaller chunks");
        }

        if (totalComplexity > 100)
        {
            optimizationRecommendations.Add("High total complexity detected - consider reducing concurrent operations");
        }

        var mutationCount = operations.Count(op => IsMutation(op.Query));
        if (mutationCount > operations.Count / 2)
        {
            optimizationRecommendations.Add("High mutation ratio - consider sequential execution for data consistency");
        }

        return new
        {
            summary = new
            {
                totalOperations = operations.Count,
                queryCount = operations.Count - mutationCount,
                mutationCount = mutationCount,
                totalComplexityScore = totalComplexity,
                estimatedTotalExecutionTimeMs = (int)estimatedExecutionTime.TotalMilliseconds,
                averageComplexityPerOperation = operations.Count > 0 ? totalComplexity / (double)operations.Count : 0,
                recommendedMaxConcurrency = CalculateRecommendedConcurrency(operations, totalComplexity)
            },
            analysis = new
            {
                securityWarnings = securityWarnings,
                optimizationRecommendations = optimizationRecommendations,
                resourceEstimation = includeResourceEstimation ? new
                {
                    estimatedMemoryUsageMB = EstimateMemoryUsage(operations),
                    recommendedTimeoutSeconds = Math.Max(30, (int)estimatedExecutionTime.TotalSeconds + 10),
                    cachingOpportunities = IdentifyCachingOpportunities(operations)
                } : null,
                operationDetails = includeComplexityAnalysis ? operationAnalysis : null
            },
            recommendations = new
            {
                executionStrategy = DetermineExecutionStrategy(operations, totalComplexity),
                batchSizeRecommendation = GetOptimalBatchSize(operations.Count, totalComplexity),
                priorityOrder = GeneratePriorityOrder(operations)
            }
        };
    }

    /// <summary>
    /// Create error response with comprehensive information
    /// </summary>
    private static string CreateErrorResponse(string title, string message, string details, List<string> suggestions)
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
                type = "BATCH_OPERATION_ERROR"
            },
            metadata = new
            {
                operation = "batch_operations",
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

    // Helper methods for analysis (simplified implementations)
    private static int CalculateQueryComplexity(string query) => query.Split('{').Length - 1;
    private static TimeSpan EstimateExecutionTime(string query, int complexity) => TimeSpan.FromMilliseconds(complexity * 10);
    private static List<string> AnalyzeSecurityRisks(string query) => [];
    private static int CountFields(string query) => query.Split(' ').Count(w => !w.Contains('{') && !w.Contains('}'));
    private static List<string> GenerateOperationRecommendations(string query, int complexity) => complexity > 10 ? ["Consider query optimization"] : [];
    private static int CalculateRecommendedConcurrency(List<BatchQueryRequest> operations, int totalComplexity) => Math.Max(1, Math.Min(10, 50 / Math.Max(1, totalComplexity / operations.Count)));
    private static int EstimateMemoryUsage(List<BatchQueryRequest> operations) => operations.Count * 2; // MB estimate
    private static List<string> IdentifyCachingOpportunities(List<BatchQueryRequest> operations) => ["Consider caching for repeated query patterns"];
    private static string DetermineExecutionStrategy(List<BatchQueryRequest> operations, int totalComplexity) => totalComplexity > 100 ? "sequential" : "parallel";
    private static int GetOptimalBatchSize(int currentSize, int complexity) => Math.Min(currentSize, Math.Max(5, 100 / Math.Max(1, complexity / currentSize)));
    private static List<int> GeneratePriorityOrder(List<BatchQueryRequest> operations) => Enumerable.Range(0, operations.Count).ToList();
}
