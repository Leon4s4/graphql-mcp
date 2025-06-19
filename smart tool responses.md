# Smart Default Responses in C# ModelContextProtocol for GraphQL MCP Servers

Based on comprehensive research into the ModelContextProtocol NuGet package, GraphQL introspection patterns, and real-world MCP implementations, this report provides detailed implementation guidance for creating comprehensive, single-call responses that minimize client-server round trips.

## Current ModelContextProtocol Package Landscape

The ModelContextProtocol C# SDK is currently in preview (version 0.2.0-preview.3) with three main packages: **ModelContextProtocol** (recommended), **ModelContextProtocol.Core** (minimal), and **ModelContextProtocol.AspNetCore** (HTTP extensions). The package provides solid foundations but lacks native GraphQL support, requiring custom implementation for comprehensive schema introspection.

**Key limitations for GraphQL integration:**
- No built-in GraphQL schema introspection
- String-based tool responses rather than structured objects
- Limited metadata handling beyond basic content annotations
- No automatic schema-to-tool conversion

## Comprehensive Schema Response Architecture

### Core Response Model Structure

The foundation for smart default responses lies in creating comprehensive response objects that include all necessary metadata in a single call:

```csharp
public class GraphQLComprehensiveResponse
{
    public SchemaIntrospectionData Schema { get; set; }
    public List<QueryExample> CommonQueries { get; set; }
    public List<MutationExample> CommonMutations { get; set; }
    public EndpointMetadata EndpointInfo { get; set; }
    public PerformanceMetadata Performance { get; set; }
    public CacheMetadata CacheInfo { get; set; }
}

public class SchemaIntrospectionData
{
    public SchemaInfo SchemaInfo { get; set; }
    public List<GraphQLTypeInfo> Types { get; set; }
    public List<DirectiveInfo> Directives { get; set; }
    public SchemaMetadata Metadata { get; set; }
}

public class GraphQLTypeInfo
{
    public TypeKind Kind { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<FieldInfo> Fields { get; set; }
    public List<InputFieldInfo> InputFields { get; set; }
    public List<TypeReference> Interfaces { get; set; }
    public List<EnumValueInfo> EnumValues { get; set; }
    
    // Smart default extensions
    public List<string> ExampleUsages { get; set; }
    public List<QueryExample> RelatedQueries { get; set; }
    public Dictionary<string, object> Extensions { get; set; }
}

public class FieldInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ArgumentInfo> Args { get; set; }
    public TypeReference Type { get; set; }
    public bool IsDeprecated { get; set; }
    public string DeprecationReason { get; set; }
    
    // Comprehensive metadata
    public List<string> ExampleValues { get; set; }
    public string UsageHint { get; set; }
    public PerformanceProfile PerformanceProfile { get; set; }
}

public class QueryExample
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Query { get; set; }
    public Dictionary<string, object> Variables { get; set; }
    public object ExpectedResult { get; set; }
    public List<string> Tags { get; set; }
    public int ComplexityScore { get; set; }
}

public class EndpointMetadata
{
    public string Url { get; set; }
    public List<string> SupportedProtocols { get; set; }
    public AuthenticationInfo Authentication { get; set; }
    public RateLimitInfo RateLimit { get; set; }
    public List<string> SupportedFeatures { get; set; }
}
```

### Implementation of IntrospectSchema Tool

Here's a comprehensive implementation that returns everything Claude might need in a single response:

```csharp
[McpServerToolType]
public class GraphQLIntrospectionTool
{
    private readonly IGraphQLSchemaService _schemaService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public GraphQLIntrospectionTool(
        IGraphQLSchemaService schemaService,
        IMemoryCache cache,
        IConfiguration config)
    {
        _schemaService = schemaService;
        _cache = cache;
        _config = config;
    }

    [McpServerTool]
    [Description("Get comprehensive GraphQL schema information with examples and metadata")]
    public async Task<string> IntrospectSchema(
        [Description("Include example queries and mutations")] bool includeExamples = true,
        [Description("Include performance metadata")] bool includePerformance = true,
        [Description("Maximum number of examples per type")] int maxExamples = 5,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"comprehensive_schema_{includeExamples}_{includePerformance}_{maxExamples}";
        
        var response = await _cache.GetOrCreateAsync(cacheKey, async factory =>
        {
            factory.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            factory.SlidingExpiration = TimeSpan.FromMinutes(5);
            
            var startTime = DateTime.UtcNow;
            
            // Parallel execution for maximum efficiency
            var tasks = new List<Task>();
            
            var schemaTask = _schemaService.GetSchemaIntrospectionAsync();
            var examplesTask = includeExamples ? 
                _schemaService.GetCommonQueriesAsync(maxExamples) : 
                Task.FromResult<List<QueryExample>>(new());
            var mutationsTask = includeExamples ? 
                _schemaService.GetCommonMutationsAsync(maxExamples) : 
                Task.FromResult<List<MutationExample>>(new());
            var endpointTask = _schemaService.GetEndpointMetadataAsync();
            
            await Task.WhenAll(schemaTask, examplesTask, mutationsTask, endpointTask);
            
            var processingTime = DateTime.UtcNow - startTime;
            
            return new GraphQLComprehensiveResponse
            {
                Schema = await schemaTask,
                CommonQueries = await examplesTask,
                CommonMutations = await mutationsTask,
                EndpointInfo = await endpointTask,
                Performance = includePerformance ? new PerformanceMetadata
                {
                    SchemaSize = (await schemaTask).Types.Count,
                    ProcessingTimeMs = (int)processingTime.TotalMilliseconds,
                    CacheHit = false,
                    LastUpdated = DateTime.UtcNow
                } : null,
                CacheInfo = new CacheMetadata
                {
                    CachedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                    CacheKey = cacheKey
                }
            };
        });

        return JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }
}
```

## Advanced Caching and Performance Optimization

### Multi-Level Caching Strategy

Implement a sophisticated caching system that optimizes for both memory usage and performance:

```csharp
public class OptimizedGraphQLSchemaService : IGraphQLSchemaService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ISchema _graphQLSchema;
    private readonly ArrayPool<byte> _arrayPool;
    private readonly RecyclableMemoryStreamManager _streamManager;

    public OptimizedGraphQLSchemaService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ISchema graphQLSchema)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _graphQLSchema = graphQLSchema;
        _arrayPool = ArrayPool<byte>.Shared;
        _streamManager = new RecyclableMemoryStreamManager();
    }

    public async Task<SchemaIntrospectionData> GetSchemaIntrospectionAsync()
    {
        const string cacheKey = "schema_introspection_v2";
        
        // L1 Cache: Memory Cache (fastest)
        if (_memoryCache.TryGetValue(cacheKey, out SchemaIntrospectionData cached))
        {
            return cached;
        }

        // L2 Cache: Distributed Cache (Redis, etc.)
        var compressedData = await _distributedCache.GetAsync(cacheKey);
        if (compressedData != null)
        {
            var decompressed = await DecompressSchemaDataAsync(compressedData);
            
            // Store in L1 cache for faster subsequent access
            _memoryCache.Set(cacheKey, decompressed, TimeSpan.FromMinutes(15));
            return decompressed;
        }

        // Cache miss: Generate schema data
        var schemaData = await GenerateSchemaIntrospectionAsync();
        
        // Store in both caches
        var compressed = await CompressSchemaDataAsync(schemaData);
        await _distributedCache.SetAsync(cacheKey, compressed, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
            SlidingExpiration = TimeSpan.FromMinutes(30)
        });
        
        _memoryCache.Set(cacheKey, schemaData, TimeSpan.FromMinutes(15));
        return schemaData;
    }

    private async Task<byte[]> CompressSchemaDataAsync(SchemaIntrospectionData data)
    {
        using var stream = _streamManager.GetStream();
        using (var gzip = new GZipStream(stream, CompressionLevel.Optimal))
        {
            await JsonSerializer.SerializeAsync(gzip, data);
        }
        return stream.ToArray();
    }

    private async Task<SchemaIntrospectionData> DecompressSchemaDataAsync(byte[] compressed)
    {
        using var compressedStream = new MemoryStream(compressed);
        using var gzip = new GZipStream(compressedStream, CompressionMode.Decompress);
        return await JsonSerializer.DeserializeAsync<SchemaIntrospectionData>(gzip);
    }

    private async Task<SchemaIntrospectionData> GenerateSchemaIntrospectionAsync()
    {
        // Use GraphQL.NET or Hot Chocolate introspection
        var introspectionQuery = @"
            query IntrospectionQuery {
                __schema {
                    queryType { name }
                    mutationType { name }
                    subscriptionType { name }
                    types { ...FullType }
                    directives {
                        name
                        description
                        locations
                        args { ...InputValue }
                    }
                }
            }
            
            fragment FullType on __Type {
                kind name description
                fields(includeDeprecated: true) {
                    name description
                    args { ...InputValue }
                    type { ...TypeRef }
                    isDeprecated
                    deprecationReason
                }
                inputFields { ...InputValue }
                interfaces { ...TypeRef }
                enumValues(includeDeprecated: true) {
                    name description isDeprecated deprecationReason
                }
                possibleTypes { ...TypeRef }
            }
            
            fragment InputValue on __InputValue {
                name description
                type { ...TypeRef }
                defaultValue
            }
            
            fragment TypeRef on __Type {
                kind name
                ofType { kind name ofType { kind name } }
            }";

        var result = await _graphQLSchema.ExecuteAsync(introspectionQuery);
        return MapIntrospectionResult(result);
    }
}
```

### Memory Management for Large Schemas

Implement memory-efficient patterns for handling large schema objects:

```csharp
public class MemoryEfficientSchemaCache
{
    private readonly ConcurrentDictionary<string, WeakReference<SchemaIntrospectionData>> _weakCache;
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _cleanupTimer;

    public MemoryEfficientSchemaCache()
    {
        _weakCache = new ConcurrentDictionary<string, WeakReference<SchemaIntrospectionData>>();
        _semaphore = new SemaphoreSlim(1, 1);
        
        // Periodic cleanup of dead weak references
        _cleanupTimer = new Timer(CleanupWeakReferences, null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public async Task<SchemaIntrospectionData> GetOrCreateAsync(
        string key, 
        Func<Task<SchemaIntrospectionData>> factory)
    {
        // Try to get from weak reference cache
        if (_weakCache.TryGetValue(key, out var weakRef) && 
            weakRef.TryGetTarget(out var cached))
        {
            return cached;
        }

        await _semaphore.WaitAsync();
        try
        {
            // Double-check pattern
            if (_weakCache.TryGetValue(key, out weakRef) && 
                weakRef.TryGetTarget(out cached))
            {
                return cached;
            }

            // Create new instance
            var newData = await factory();
            _weakCache[key] = new WeakReference<SchemaIntrospectionData>(newData);
            return newData;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void CleanupWeakReferences(object state)
    {
        var keysToRemove = new List<string>();
        
        foreach (var kvp in _weakCache)
        {
            if (!kvp.Value.TryGetTarget(out _))
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _weakCache.TryRemove(key, out _);
        }
    }
}
```

## Comprehensive Tool Response Patterns

### Smart Default Response Implementation

Create tools that anticipate what information Claude will need and provide it proactively:

```csharp
[McpServerTool]
[Description("Execute GraphQL query with comprehensive metadata and suggestions")]
public async Task<string> ExecuteGraphQLQuery(
    [Description("GraphQL query to execute")] string query,
    [Description("Variables for the query")] string variables = "{}",
    [Description("Include query analysis and suggestions")] bool includeSuggestions = true,
    [Description("Include performance metrics")] bool includeMetrics = true,
    CancellationToken cancellationToken = default)
{
    var startTime = DateTime.UtcNow;
    var queryId = Guid.NewGuid().ToString("N")[..8];
    
    try
    {
        // Parse variables
        var variableDict = JsonSerializer.Deserialize<Dictionary<string, object>>(variables);
        
        // Execute query with parallel metadata gathering
        var executeTask = _graphQLExecutor.ExecuteAsync(query, variableDict);
        var analysisTask = includeSuggestions ? 
            _queryAnalyzer.AnalyzeQueryAsync(query) : 
            Task.FromResult<QueryAnalysis>(null);
        
        await Task.WhenAll(executeTask, analysisTask);
        
        var result = await executeTask;
        var analysis = await analysisTask;
        var executionTime = DateTime.UtcNow - startTime;

        // Build comprehensive response
        var response = new GraphQLExecutionResponse
        {
            QueryId = queryId,
            Data = result.Data,
            Errors = result.Errors?.Select(e => new ExecutionError
            {
                Message = e.Message,
                Path = e.Path,
                Extensions = e.Extensions,
                Suggestions = GetErrorSuggestions(e)
            }).ToList(),
            
            Metadata = new ExecutionMetadata
            {
                ExecutionTimeMs = (int)executionTime.TotalMilliseconds,
                ComplexityScore = analysis?.ComplexityScore,
                DepthScore = analysis?.DepthScore,
                FieldCount = analysis?.FieldCount,
                CacheHit = result.Extensions?.ContainsKey("cacheHit") == true,
                DataFreshness = GetDataFreshness(result)
            },

            // Smart suggestions based on query and results
            Suggestions = includeSuggestions ? new QuerySuggestions
            {
                OptimizationHints = analysis?.OptimizationHints ?? new List<string>(),
                RelatedQueries = await GetRelatedQueriesAsync(query),
                FieldSuggestions = GetFieldSuggestions(query, result),
                PaginationHints = GetPaginationHints(result)
            } : null,

            // Include schema context for referenced types
            SchemaContext = ExtractSchemaContext(query, result),
            
            // Performance recommendations
            Performance = includeMetrics ? new PerformanceRecommendations
            {
                ShouldCache = ShouldCacheQuery(analysis),
                OptimalPagination = GetOptimalPagination(analysis),
                IndexHints = GetIndexHints(analysis),
                QueryComplexityRating = GetComplexityRating(analysis)
            } : null
        };

        return JsonSerializer.Serialize(response, _jsonOptions);
    }
    catch (Exception ex)
    {
        // Comprehensive error response
        var errorResponse = new GraphQLExecutionResponse
        {
            QueryId = queryId,
            Errors = new List<ExecutionError>
            {
                new()
                {
                    Message = ex.Message,
                    Extensions = new Dictionary<string, object>
                    {
                        ["errorType"] = ex.GetType().Name,
                        ["timestamp"] = DateTime.UtcNow,
                        ["queryId"] = queryId
                    },
                    Suggestions = GetExceptionSuggestions(ex, query)
                }
            },
            Metadata = new ExecutionMetadata
            {
                ExecutionTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                Failed = true
            }
        };

        return JsonSerializer.Serialize(errorResponse, _jsonOptions);
    }
}

public class GraphQLExecutionResponse
{
    public string QueryId { get; set; }
    public object Data { get; set; }
    public List<ExecutionError> Errors { get; set; }
    public ExecutionMetadata Metadata { get; set; }
    public QuerySuggestions Suggestions { get; set; }
    public SchemaContext SchemaContext { get; set; }
    public PerformanceRecommendations Performance { get; set; }
}

public class QuerySuggestions
{
    public List<string> OptimizationHints { get; set; }
    public List<QueryExample> RelatedQueries { get; set; }
    public List<string> FieldSuggestions { get; set; }
    public PaginationHints PaginationHints { get; set; }
}

public class SchemaContext
{
    public List<GraphQLTypeInfo> ReferencedTypes { get; set; }
    public List<string> AvailableFields { get; set; }
    public List<string> RequiredArguments { get; set; }
    public Dictionary<string, List<string>> EnumValues { get; set; }
}
```

### Batch Operations for Efficiency

Implement batch operations that handle multiple requests in a single call:

```csharp
[McpServerTool]
[Description("Execute multiple GraphQL operations in a single batch")]
public async Task<string> ExecuteBatchQueries(
    [Description("Array of GraphQL queries to execute")] string queriesJson,
    [Description("Maximum concurrent executions")] int maxConcurrency = 5,
    CancellationToken cancellationToken = default)
{
    var batchId = Guid.NewGuid().ToString("N")[..8];
    var startTime = DateTime.UtcNow;
    
    var queries = JsonSerializer.Deserialize<List<BatchQueryRequest>>(queriesJson);
    
    // Execute with controlled concurrency
    var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    var results = new ConcurrentBag<BatchQueryResult>();
    
    var tasks = queries.Select(async (query, index) =>
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var queryStart = DateTime.UtcNow;
            var result = await _graphQLExecutor.ExecuteAsync(query.Query, query.Variables);
            var queryTime = DateTime.UtcNow - queryStart;
            
            results.Add(new BatchQueryResult
            {
                Index = index,
                QueryId = query.Id ?? $"{batchId}_{index}",
                Data = result.Data,
                Errors = result.Errors?.Select(e => e.Message).ToList(),
                ExecutionTimeMs = (int)queryTime.TotalMilliseconds,
                Success = result.Errors?.Count == 0
            });
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
    
    var totalTime = DateTime.UtcNow - startTime;
    var orderedResults = results.OrderBy(r => r.Index).ToList();
    
    var batchResponse = new BatchExecutionResponse
    {
        BatchId = batchId,
        Results = orderedResults,
        Summary = new BatchSummary
        {
            TotalQueries = queries.Count,
            SuccessfulQueries = orderedResults.Count(r => r.Success),
            FailedQueries = orderedResults.Count(r => !r.Success),
            TotalExecutionTimeMs = (int)totalTime.TotalMilliseconds,
            AverageQueryTimeMs = orderedResults.Average(r => r.ExecutionTimeMs),
            MaxConcurrency = maxConcurrency
        }
    };
    
    return JsonSerializer.Serialize(batchResponse, _jsonOptions);
}
```

## Integration with ModelContextProtocol

### Dependency Injection Configuration

Set up the MCP server with all necessary services for comprehensive GraphQL support:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add MCP Server with GraphQL extensions
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Add GraphQL services
builder.Services.AddGraphQL()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AllowIntrospection(!builder.Environment.IsProduction())
    .EnablePersistedQueries()
    .AddInMemoryQueryStorage();

// Add comprehensive caching
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
    options.CompactionPercentage = 0.25;
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Register custom services
builder.Services.AddSingleton<IGraphQLSchemaService, OptimizedGraphQLSchemaService>();
builder.Services.AddSingleton<IQueryAnalyzer, QueryAnalyzer>();
builder.Services.AddSingleton<MemoryEfficientSchemaCache>();
builder.Services.AddSingleton<RecyclableMemoryStreamManager>();

var app = builder.Build();

// Configure MCP server
app.MapMcpServer();

app.Run();
```

### Custom Content Type for Structured Responses

Extend the ModelContextProtocol to support structured GraphQL responses:

```csharp
public static class McpContentExtensions
{
    public static Content CreateGraphQLResponse(object data, GraphQLResponseMetadata metadata = null)
    {
        var response = new
        {
            data = data,
            metadata = metadata ?? new GraphQLResponseMetadata
            {
                Timestamp = DateTime.UtcNow,
                Version = "1.0"
            }
        };

        return new Content
        {
            Type = "application/json",
            Text = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            Annotations = new ContentAnnotations
            {
                Audience = [ContentAudience.User],
                Priority = ContentPriority.Normal
            }
        };
    }

    public static Content CreateGraphQLError(string message, string code = null, object details = null)
    {
        var error = new
        {
            error = new
            {
                message = message,
                code = code ?? "GRAPHQL_ERROR",
                details = details,
                timestamp = DateTime.UtcNow
            }
        };

        return new Content
        {
            Type = "application/json",
            Text = JsonSerializer.Serialize(error, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            Annotations = new ContentAnnotations
            {
                Audience = [ContentAudience.User],
                Priority = ContentPriority.High
            }
        };
    }
}
```

## Performance Considerations and Monitoring

### Comprehensive Performance Monitoring

Implement detailed performance tracking for optimization:

```csharp
public class GraphQLPerformanceMonitor
{
    private readonly ILogger<GraphQLPerformanceMonitor> _logger;
    private readonly IMetrics _metrics;
    private readonly ConcurrentDictionary<string, PerformanceStats> _queryStats;

    public GraphQLPerformanceMonitor(ILogger<GraphQLPerformanceMonitor> logger, IMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
        _queryStats = new ConcurrentDictionary<string, PerformanceStats>();
    }

    public async Task<T> MonitorOperation<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var startMemory = GC.GetTotalMemory(false);
        
        try
        {
            var result = await operation();
            
            stopwatch.Stop();
            var endMemory = GC.GetTotalMemory(false);
            var memoryUsed = endMemory - startMemory;
            
            RecordPerformanceMetrics(operationName, stopwatch.ElapsedMilliseconds, memoryUsed, true);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            RecordPerformanceMetrics(operationName, stopwatch.ElapsedMilliseconds, 0, false);
            
            _logger.LogError(ex, "Operation {OperationName} failed after {ElapsedMs}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void RecordPerformanceMetrics(string operationName, long elapsedMs, long memoryUsed, bool success)
    {
        _metrics.Measure.Counter.Increment("graphql_operations_total", 
            new MetricTags("operation", operationName, "success", success.ToString()));
        
        _metrics.Measure.Histogram.Update("graphql_operation_duration_ms", elapsedMs,
            new MetricTags("operation", operationName));
        
        if (memoryUsed > 0)
        {
            _metrics.Measure.Histogram.Update("graphql_operation_memory_bytes", memoryUsed,
                new MetricTags("operation", operationName));
        }

        // Update in-memory stats for adaptive optimization
        _queryStats.AddOrUpdate(operationName, 
            new PerformanceStats { TotalExecutions = 1, TotalTime = elapsedMs, SuccessCount = success ? 1 : 0 },
            (key, existing) => new PerformanceStats
            {
                TotalExecutions = existing.TotalExecutions + 1,
                TotalTime = existing.TotalTime + elapsedMs,
                SuccessCount = existing.SuccessCount + (success ? 1 : 0),
                AverageTime = (existing.TotalTime + elapsedMs) / (existing.TotalExecutions + 1)
            });
    }

    public Dictionary<string, PerformanceStats> GetPerformanceStats() => 
        new Dictionary<string, PerformanceStats>(_queryStats);
}
```

This comprehensive implementation provides the foundation for creating smart default responses in C# ModelContextProtocol GraphQL MCP servers. The approach minimizes client-server round trips by proactively including relevant metadata, examples, and contextual information in every response while maintaining optimal performance through sophisticated caching and memory management strategies.

**Key benefits of this implementation:**
- **Single-call completeness**: Responses include all likely needed information
- **Intelligent caching**: Multi-level caching with memory optimization
- **Performance monitoring**: Built-in metrics and adaptive optimization
- **Extensible architecture**: Easy to add new response types and metadata
- **Production-ready**: Comprehensive error handling and resource management