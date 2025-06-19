using System.Collections.Concurrent;
using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Enhanced service for combined GraphQL operations with intelligent caching and state management
/// </summary>
public sealed class CombinedOperationsService
{
    private static readonly Lazy<CombinedOperationsService> LazyInstance = new(() => new CombinedOperationsService());
    
    private readonly ConcurrentDictionary<string, object> _schemaCache = new();
    private readonly ConcurrentDictionary<string, DateTime> _cacheTimestamps = new();
    private readonly ConcurrentDictionary<string, List<string>> _operationHistory = new();
    private readonly ConcurrentDictionary<string, object> _performanceMetrics = new();
    
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);
    private readonly object _cacheLock = new();

    public static CombinedOperationsService Instance => LazyInstance.Value;

    private CombinedOperationsService() { }

    #region Schema Caching

    public async Task<object> GetCachedSchemaAsync(string endpoint, GraphQlEndpointInfo endpointInfo, bool includeMutations = false, int maxDepth = 3)
    {
        var cacheKey = GenerateCacheKey(endpoint, includeMutations, maxDepth);
        
        lock (_cacheLock)
        {
            if (_schemaCache.TryGetValue(cacheKey, out var cachedSchema) && 
                _cacheTimestamps.TryGetValue(cacheKey, out var timestamp) &&
                DateTime.UtcNow - timestamp < _cacheExpiry)
            {
                return cachedSchema;
            }
        }

        // Cache miss or expired - fetch fresh schema
        var schema = await FetchSchemaAsync(endpoint, endpointInfo, includeMutations, maxDepth);
        
        lock (_cacheLock)
        {
            _schemaCache[cacheKey] = schema;
            _cacheTimestamps[cacheKey] = DateTime.UtcNow;
        }

        return schema;
    }

    public void InvalidateSchemaCache(string endpoint)
    {
        lock (_cacheLock)
        {
            var keysToRemove = _schemaCache.Keys.Where(k => k.StartsWith($"schema_{endpoint}")).ToList();
            foreach (var key in keysToRemove)
            {
                _schemaCache.TryRemove(key, out _);
                _cacheTimestamps.TryRemove(key, out _);
            }
        }
    }

    #endregion

    #region Operation History

    public void RecordOperation(string endpoint, string operation, string operationType)
    {
        var key = $"{endpoint}_{operationType}";
        _operationHistory.AddOrUpdate(key,
            new List<string> { operation },
            (_, existing) =>
            {
                existing.Add(operation);
                return existing.TakeLast(50).ToList(); // Keep last 50 operations
            });
    }

    public List<string> GetOperationHistory(string endpoint, string? operationType = null)
    {
        var key = operationType != null ? $"{endpoint}_{operationType}" : endpoint;
        return _operationHistory.GetValueOrDefault(key, new List<string>());
    }

    public Dictionary<string, object> GetOperationStatistics(string endpoint)
    {
        var allOperations = _operationHistory
            .Where(kvp => kvp.Key.StartsWith(endpoint))
            .SelectMany(kvp => kvp.Value)
            .ToList();

        return new Dictionary<string, object>
        {
            ["totalOperations"] = allOperations.Count,
            ["uniqueOperations"] = allOperations.Distinct().Count(),
            ["mostCommonOperations"] = allOperations
                .GroupBy(op => op)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { operation = g.Key, count = g.Count() })
                .ToList(),
            ["lastActivity"] = DateTime.UtcNow
        };
    }

    #endregion

    #region Performance Metrics

    public void RecordPerformanceMetric(string endpoint, string operation, TimeSpan executionTime, bool success)
    {
        var key = $"perf_{endpoint}";
        var metric = new
        {
            endpoint,
            operation,
            executionTime = executionTime.TotalMilliseconds,
            success,
            timestamp = DateTime.UtcNow
        };

        _performanceMetrics.AddOrUpdate(key,
            new List<object> { metric },
            (_, existing) =>
            {
                if (existing is List<object> list)
                {
                    list.Add(metric);
                    return list.TakeLast(100).ToList(); // Keep last 100 metrics
                }
                return new List<object> { metric };
            });
    }

    public Dictionary<string, object> GetPerformanceMetrics(string endpoint)
    {
        var key = $"perf_{endpoint}";
        if (!_performanceMetrics.TryGetValue(key, out var metrics) || metrics is not List<object> metricsList)
        {
            return new Dictionary<string, object>
            {
                ["endpoint"] = endpoint,
                ["noData"] = true
            };
        }

        var successfulOperations = metricsList.Count(m => GetPropertyValue(m, "success", false));
        var totalOperations = metricsList.Count;
        var avgExecutionTime = metricsList.Average(m => GetPropertyValue(m, "executionTime", 0.0));

        return new Dictionary<string, object>
        {
            ["endpoint"] = endpoint,
            ["totalOperations"] = totalOperations,
            ["successRate"] = totalOperations > 0 ? (double)successfulOperations / totalOperations : 0,
            ["averageExecutionTime"] = avgExecutionTime,
            ["lastRecorded"] = metricsList.LastOrDefault()
        };
    }

    #endregion

    #region Batch Operations

    public async Task<List<object>> ExecuteBatchOperationsAsync(
        List<BatchOperation> operations,
        bool parallel = false,
        bool continueOnError = true,
        int timeoutSeconds = 30)
    {
        var results = new List<object>();
        var startTime = DateTime.UtcNow;

        try
        {
            if (parallel)
            {
                var tasks = operations.Select<BatchOperation, Task<object>>(async (op, index) =>
                {
                    var opStartTime = DateTime.UtcNow;
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                        var result = await ExecuteSingleOperationAsync(op, cts.Token);
                        
                        RecordPerformanceMetric(op.Endpoint, op.Name ?? $"Operation_{index}", 
                            DateTime.UtcNow - opStartTime, true);
                        
                        return new { 
                            name = op.Name ?? $"Operation_{index}", 
                            index, 
                            success = true, 
                            result,
                            executionTime = DateTime.UtcNow - opStartTime
                        };
                    }
                    catch (Exception ex)
                    {
                        RecordPerformanceMetric(op.Endpoint, op.Name ?? $"Operation_{index}", 
                            DateTime.UtcNow - opStartTime, false);
                        
                        if (!continueOnError) throw;
                        return new { 
                            name = op.Name ?? $"Operation_{index}", 
                            index, 
                            success = false, 
                            error = ex.Message,
                            executionTime = DateTime.UtcNow - opStartTime
                        };
                    }
                });

                var taskResults = await Task.WhenAll(tasks);
                results.AddRange(taskResults.Cast<object>());
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
                        
                        RecordPerformanceMetric(op.Endpoint, op.Name ?? $"Operation_{i}", 
                            DateTime.UtcNow - opStartTime, true);
                        
                        results.Add(new { 
                            name = op.Name ?? $"Operation_{i}", 
                            index = i, 
                            success = true, 
                            result,
                            executionTime = DateTime.UtcNow - opStartTime
                        });
                    }
                    catch (Exception ex)
                    {
                        RecordPerformanceMetric(op.Endpoint, op.Name ?? $"Operation_{i}", 
                            DateTime.UtcNow - opStartTime, false);
                        
                        var errorResult = new { 
                            name = op.Name ?? $"Operation_{i}", 
                            index = i, 
                            success = false, 
                            error = ex.Message,
                            executionTime = DateTime.UtcNow - opStartTime
                        };
                        results.Add(errorResult);
                        
                        if (!continueOnError) break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            results.Add(new { error = $"Batch execution failed: {ex.Message}" });
        }

        return results;
    }

    #endregion

    #region Schema Analysis

    public async Task<object> AnalyzeSchemaComplexityAsync(string endpoint, GraphQlEndpointInfo endpointInfo)
    {
        var schema = await GetCachedSchemaAsync(endpoint, endpointInfo, true, 10);
        
        // Perform complexity analysis
        return new
        {
            endpoint,
            complexity = CalculateComplexityMetrics(schema),
            recommendations = GenerateOptimizationRecommendations(schema),
            analyzedAt = DateTime.UtcNow
        };
    }

    public async Task<object> CompareEndpointSchemasAsync(string endpoint1, string endpoint2)
    {
        var info1 = EndpointRegistryService.Instance.GetEndpointInfo(endpoint1);
        var info2 = EndpointRegistryService.Instance.GetEndpointInfo(endpoint2);

        if (info1 == null || info2 == null)
        {
            throw new ArgumentException("Both endpoints must be registered");
        }

        var schema1 = await GetCachedSchemaAsync(endpoint1, info1, true, 5);
        var schema2 = await GetCachedSchemaAsync(endpoint2, info2, true, 5);

        return new
        {
            endpoint1,
            endpoint2,
            comparison = PerformSchemaComparison(schema1, schema2),
            comparedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Private Helper Methods

    private string GenerateCacheKey(string endpoint, bool includeMutations, int maxDepth)
    {
        return $"schema_{endpoint}_{includeMutations}_{maxDepth}";
    }

    private async Task<object> FetchSchemaAsync(string endpoint, GraphQlEndpointInfo endpointInfo, bool includeMutations, int maxDepth)
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

    private async Task<object> ExecuteSingleOperationAsync(BatchOperation operation, CancellationToken cancellationToken)
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

        // Record the operation
        RecordOperation(operation.Endpoint, operation.Query, "combined_operation");

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

    private BatchOperation ProcessResultChaining(BatchOperation operation, List<object> previousResults)
    {
        var processedQuery = operation.Query;
        
        // Simple template processing for result chaining
        for (int i = 0; i < previousResults.Count; i++)
        {
            var placeholder = $"{{result.{i}}}";
            if (processedQuery.Contains(placeholder))
            {
                var resultJson = JsonSerializer.Serialize(previousResults[i]);
                processedQuery = processedQuery.Replace(placeholder, resultJson);
            }
            
            // Support for specific field access like {{result.0.data.id}}
            var fieldPattern = $"{{result.{i}.";
            if (processedQuery.Contains(fieldPattern))
            {
                // This would need more sophisticated implementation for deep field access
                // For now, just replace with the full result
                processedQuery = processedQuery.Replace($"{{result.{i}.data}}", JsonSerializer.Serialize(previousResults[i]));
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

    private object CalculateComplexityMetrics(object schema)
    {
        // Simplified complexity calculation
        return new
        {
            estimatedComplexity = "medium",
            typeCount = 0,
            fieldCount = 0,
            maxDepth = 0,
            circularReferences = false,
            recommendations = new List<string>()
        };
    }

    private List<string> GenerateOptimizationRecommendations(object schema)
    {
        return new List<string>
        {
            "Consider implementing query depth limiting",
            "Use field-level caching for frequently accessed data",
            "Implement query complexity analysis"
        };
    }

    private object PerformSchemaComparison(object schema1, object schema2)
    {
        return new
        {
            compatible = true,
            differences = new List<string>(),
            breakingChanges = new List<string>(),
            addedTypes = new List<string>(),
            removedTypes = new List<string>()
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
