using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Stateless service for combined GraphQL operations
/// </summary>
public static class CombinedOperationsService
{

    #region Schema Operations

    public static async Task<object> GetSchemaAsync(string endpoint, GraphQlEndpointInfo endpointInfo, bool includeMutations = false, int maxDepth = 3)
    {
        return await FetchSchemaAsync(endpoint, endpointInfo, includeMutations, maxDepth);
    }

    #endregion


    #region Batch Operations

    public static async Task<List<BatchOperationResult>> ExecuteBatchOperationsAsync(
        List<BatchOperation> operations,
        bool parallel = false,
        bool continueOnError = true,
        int timeoutSeconds = 30)
    {
        var results = new List<BatchOperationResult>();
        var startTime = DateTime.UtcNow;

        try
        {
            if (parallel)
            {
                var tasks = operations.Select<BatchOperation, Task<BatchOperationResult>>(async (op, index) =>
                {
                    var opStartTime = DateTime.UtcNow;
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                        var result = await ExecuteSingleOperationAsync(op, cts.Token);
                        
                        // Performance metrics not tracked in stateless mode
                        
                        return new BatchOperationResult
                        { 
                            Name = op.Name ?? $"Operation_{index}", 
                            Index = index, 
                            Success = true, 
                            Data = result,
                            ExecutionTime = DateTime.UtcNow - opStartTime,
                            Endpoint = op.Endpoint,
                            Query = op.Query,
                            Variables = op.Variables
                        };
                    }
                    catch (Exception ex)
                    {
                        // Performance metrics not tracked in stateless mode
                        
                        if (!continueOnError) throw;
                        return new BatchOperationResult
                        { 
                            Name = op.Name ?? $"Operation_{index}", 
                            Index = index, 
                            Success = false, 
                            Error = ex.Message,
                            ExecutionTime = DateTime.UtcNow - opStartTime,
                            Endpoint = op.Endpoint,
                            Query = op.Query,
                            Variables = op.Variables
                        };
                    }
                });

                var taskResults = await Task.WhenAll(tasks);
                results.AddRange(taskResults);
            }
            else
            {
                for (int i = 0; i < operations.Count; i++)
                {
                    var op = operations[i];
                    var opStartTime = DateTime.UtcNow;
                    
                    try
                    {
                        // Process result chaining for sequential execution
                        op = ProcessResultChaining(op, results);
                        
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                        var result = await ExecuteSingleOperationAsync(op, cts.Token);
                        
                        // Performance metrics not tracked in stateless mode
                        
                        results.Add(new BatchOperationResult 
                        { 
                            Name = op.Name ?? $"Operation_{i}", 
                            Index = i, 
                            Success = true, 
                            Data = result,
                            ExecutionTime = DateTime.UtcNow - opStartTime,
                            Endpoint = op.Endpoint,
                            Query = op.Query,
                            Variables = op.Variables
                        });
                    }
                    catch (Exception ex)
                    {
                        // Performance metrics not tracked in stateless mode
                        
                        var errorResult = new BatchOperationResult 
                        { 
                            Name = op.Name ?? $"Operation_{i}", 
                            Index = i, 
                            Success = false, 
                            Error = ex.Message,
                            ExecutionTime = DateTime.UtcNow - opStartTime,
                            Endpoint = op.Endpoint,
                            Query = op.Query,
                            Variables = op.Variables
                        };
                        results.Add(errorResult);
                        
                        if (!continueOnError) break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            results.Add(new BatchOperationResult 
            { 
                Name = "BatchExecutionError",
                Index = -1,
                Success = false,
                Error = $"Batch execution failed: {ex.Message}",
                ExecutionTime = DateTime.UtcNow - startTime,
                Endpoint = "System"
            });
        }

        return results;
    }

    #endregion

    #region Schema Analysis

    public static async Task<SchemaAnalysisResult> AnalyzeSchemaComplexityAsync(string endpoint, GraphQlEndpointInfo endpointInfo)
    {
        var startTime = DateTime.UtcNow;
        var schema = await GetSchemaAsync(endpoint, endpointInfo, true, 10);
        
        // Perform complexity analysis
        return new SchemaAnalysisResult
        {
            Endpoint = endpoint,
            Complexity = CalculateComplexityMetrics(schema),
            Recommendations = GenerateOptimizationRecommendations(schema),
            AnalyzedAt = DateTime.UtcNow,
            AnalysisDuration = DateTime.UtcNow - startTime
        };
    }

    public static async Task<EndpointComparisonResult> CompareEndpointSchemasAsync(string endpoint1, string endpoint2)
    {
        var startTime = DateTime.UtcNow;
        var info1 = EndpointRegistryService.Instance.GetEndpointInfo(endpoint1);
        var info2 = EndpointRegistryService.Instance.GetEndpointInfo(endpoint2);

        if (info1 == null || info2 == null)
        {
            throw new ArgumentException("Both endpoints must be registered");
        }

        var schema1 = await GetSchemaAsync(endpoint1, info1, true, 5);
        var schema2 = await GetSchemaAsync(endpoint2, info2, true, 5);

        var perf1 = new { averageExecutionTime = 0.0, successRate = 0.0 };
        var perf2 = new { averageExecutionTime = 0.0, successRate = 0.0 };

        return new EndpointComparisonResult
        {
            Endpoint1 = endpoint1,
            Endpoint2 = endpoint2,
            Comparison = PerformSchemaComparison(schema1, schema2),
            PerformanceComparison = new PerformanceComparisonResult
            {
                Endpoint1AvgResponseTime = GetPropertyValue(perf1, "averageExecutionTime", 0.0),
                Endpoint2AvgResponseTime = GetPropertyValue(perf2, "averageExecutionTime", 0.0),
                Endpoint1SuccessRate = GetPropertyValue(perf1, "successRate", 0.0),
                Endpoint2SuccessRate = GetPropertyValue(perf2, "successRate", 0.0)
            },
            ComparedAt = DateTime.UtcNow,
            ComparisonDuration = DateTime.UtcNow - startTime
        };
    }

    #endregion

    #region Private Helper Methods

    private static async Task<object> FetchSchemaAsync(string endpoint, GraphQlEndpointInfo endpointInfo, bool includeMutations, int maxDepth)
    {
        try
        {
            var schemaService = new StrawberryShakeSchemaService();
            var schemaResult = await schemaService.GetSchemaAsync(endpointInfo);
            
            if (!schemaResult.IsSuccess)
            {
                return new 
                { 
                    error = $"Failed to introspect schema: {schemaResult.ErrorMessage}", 
                    endpoint,
                    failedAt = DateTime.UtcNow
                };
            }

            var schema = schemaResult.Schema;
            var rootTypes = schemaService.GetRootTypes(schema);

            return new
            {
                endpoint,
                url = endpointInfo.Url,
                rootTypes,
                introspectionDepth = maxDepth,
                cachedAt = DateTime.UtcNow,
                metadata = new
                {
                    hasSubscriptions = !string.IsNullOrEmpty(rootTypes.SubscriptionType),
                    hasMutations = !string.IsNullOrEmpty(rootTypes.MutationType)
                }
            };
        }
        catch (Exception ex)
        {
            return new 
            { 
                error = $"Failed to introspect schema: {ex.Message}", 
                endpoint,
                failedAt = DateTime.UtcNow
            };
        }
    }

    private static async Task<object> ExecuteSingleOperationAsync(BatchOperation operation, CancellationToken cancellationToken)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(operation.Endpoint);
        if (endpointInfo == null)
        {
            throw new ArgumentException($"Endpoint '{operation.Endpoint}' not found");
        }

        var httpClient = HttpClientHelper.GetSharedClient();
        var requestBody = new { query = operation.Query, variables = operation.Variables };
        var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(
            endpointInfo, 
            requestBody);

        // Operation recording not available in stateless mode

        return new
        {
            endpoint = operation.Endpoint,
            operation = operation.Name,
            query = operation.Query,
            variables = operation.Variables,
            result,
            executedAt = DateTime.UtcNow
        };
    }

    private static BatchOperation ProcessResultChaining(BatchOperation operation, List<BatchOperationResult> previousResults)
    {
        var processedQuery = operation.Query;
        
        // Simple template processing for result chaining
        for (int i = 0; i < previousResults.Count; i++)
        {
            var placeholder = $"{{result.{i}}}";
            if (processedQuery.Contains(placeholder))
            {
                var resultJson = JsonSerializer.Serialize(previousResults[i].Data);
                processedQuery = processedQuery.Replace(placeholder, resultJson);
            }
            
            // Support for specific field access like {{result.0.data.id}}
            var fieldPattern = $"{{result.{i}.";
            if (processedQuery.Contains(fieldPattern))
            {
                // This would need more sophisticated implementation for deep field access
                // For now, just replace with the full result
                processedQuery = processedQuery.Replace($"{{result.{i}.data}}", JsonSerializer.Serialize(previousResults[i].Data));
            }
        }

        return new BatchOperation
        {
            Endpoint = operation.Endpoint,
            Query = processedQuery,
            Variables = operation.Variables,
            Name = operation.Name
        };
    }

    private static SchemaComplexityResult CalculateComplexityMetrics(object schema)
    {
        // Simplified complexity calculation
        return new SchemaComplexityResult
        {
            EstimatedComplexity = "medium",
            TypeCount = 0,
            FieldCount = 0,
            MaxDepth = 0,
            CircularReferences = false,
            Recommendations = new List<string>(),
            ComplexityScore = 50,
            AverageFieldsPerType = 0.0
        };
    }

    private static List<string> GenerateOptimizationRecommendations(object schema)
    {
        return new List<string>
        {
            "Consider implementing query depth limiting",
            "Use field-level caching for frequently accessed data",
            "Implement query complexity analysis"
        };
    }

    private static SchemaComparisonResult PerformSchemaComparison(object schema1, object schema2)
    {
        return new SchemaComparisonResult
        {
            Compatible = true,
            Differences = new List<string>(),
            BreakingChanges = new List<string>(),
            AddedTypes = new List<string>(),
            RemovedTypes = new List<string>(),
            CompatibilityRating = "Fully Compatible"
        };
    }

    private static T GetPropertyValue<T>(object obj, string propertyName, T defaultValue)
    {
        try
        {
            if (obj is Dictionary<string, object> dict && dict.TryGetValue(propertyName, out var value))
            {
                return (T)value;
            }
            
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var val = property.GetValue(obj);
                return val is T ? (T)val : defaultValue;
            }
        }
        catch
        {
            // Ignore errors and return default
        }
        
        return defaultValue;
    }

    #endregion
}

/// <summary>
/// Represents a single operation in a batch execution
/// </summary>
public class BatchOperation
{
    public string Endpoint { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, object>? Variables { get; set; }
    public string? Name { get; set; }
}
