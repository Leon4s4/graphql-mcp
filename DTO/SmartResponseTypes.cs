using System.Text.Json.Serialization;

namespace Graphql.Mcp.DTO;

/// <summary>
/// Comprehensive GraphQL response with smart defaults and metadata
/// </summary>
public class GraphQLComprehensiveResponse
{
    public SchemaIntrospectionData? Schema { get; set; }
    public List<QueryExample> CommonQueries { get; set; } = [];
    public List<MutationExample> CommonMutations { get; set; } = [];
    public EndpointMetadata? EndpointInfo { get; set; }
    public PerformanceMetadata? Performance { get; set; }
    public CacheMetadata? CacheInfo { get; set; }
    public List<string> RecommendedActions { get; set; } = [];
    public Dictionary<string, object> Extensions { get; set; } = new();
}

/// <summary>
/// Enhanced schema introspection data with smart defaults
/// </summary>
public class SchemaIntrospectionData
{
    public SchemaInfo SchemaInfo { get; set; } = new();
    public List<GraphQLTypeInfo> Types { get; set; } = [];
    public List<DirectiveInfo> Directives { get; set; } = [];
    public SchemaMetadata Metadata { get; set; } = new();
    public TypeRelationships TypeRelationships { get; set; } = new();
    public List<string> AvailableOperations { get; set; } = [];
}

/// <summary>
/// Enhanced type information with usage examples and metadata
/// </summary>
public class GraphQLTypeInfo
{
    public TypeKind Kind { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<FieldInfo> Fields { get; set; } = [];
    public List<InputFieldInfo> InputFields { get; set; } = [];
    public List<TypeReference> Interfaces { get; set; } = [];
    public List<EnumValueInfo> EnumValues { get; set; } = [];
    
    // Smart default extensions
    public List<string> ExampleUsages { get; set; } = [];
    public List<QueryExample> RelatedQueries { get; set; } = [];
    public Dictionary<string, object> Extensions { get; set; } = new();
    public UsageStatistics? UsageStats { get; set; }
    public ComplexityMetrics? Complexity { get; set; }
}

/// <summary>
/// Enhanced field information with comprehensive metadata
/// </summary>
public class FieldInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<ArgumentInfo> Args { get; set; } = [];
    public TypeReference Type { get; set; } = new();
    public bool IsDeprecated { get; set; }
    public string? DeprecationReason { get; set; }
    
    // Comprehensive metadata
    public List<string> ExampleValues { get; set; } = [];
    public string? UsageHint { get; set; }
    public PerformanceProfile? PerformanceProfile { get; set; }
    public List<string> ValidationRules { get; set; } = [];
    public SecurityInfo? Security { get; set; }
}

/// <summary>
/// Query example with comprehensive metadata
/// </summary>
public class QueryExample
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Query { get; set; } = "";
    public Dictionary<string, object> Variables { get; set; } = new();
    public object? ExpectedResult { get; set; }
    public List<string> Tags { get; set; } = [];
    public int ComplexityScore { get; set; }
    public TimeSpan EstimatedExecutionTime { get; set; }
    public List<string> RequiredPermissions { get; set; } = [];
    public PaginationInfo? Pagination { get; set; }
}

/// <summary>
/// Mutation example with smart defaults
/// </summary>
public class MutationExample
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Mutation { get; set; } = "";
    public Dictionary<string, object> Variables { get; set; } = new();
    public object? ExpectedResult { get; set; }
    public List<string> Tags { get; set; } = [];
    public int ComplexityScore { get; set; }
    public List<string> SideEffects { get; set; } = [];
    public List<string> RequiredPermissions { get; set; } = [];
    public bool IsIdempotent { get; set; }
}

/// <summary>
/// Comprehensive endpoint metadata
/// </summary>
public class EndpointMetadata
{
    public string Url { get; set; } = "";
    public List<string> SupportedProtocols { get; set; } = [];
    public AuthenticationInfo? Authentication { get; set; }
    public RateLimitInfo? RateLimit { get; set; }
    public List<string> SupportedFeatures { get; set; } = [];
    public HealthStatus Health { get; set; } = new();
    public VersionInfo Version { get; set; } = new();
    public List<string> DeprecationWarnings { get; set; } = [];
}

/// <summary>
/// Performance metadata for operations
/// </summary>
public class PerformanceMetadata
{
    public int SchemaSize { get; set; }
    public int ProcessingTimeMs { get; set; }
    public bool CacheHit { get; set; }
    public DateTime LastUpdated { get; set; }
    public MemoryUsage? MemoryUsage { get; set; }
    public List<PerformanceRecommendation> Recommendations { get; set; } = [];
}

/// <summary>
/// Cache metadata information
/// </summary>
public class CacheMetadata
{
    public DateTime CachedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string CacheKey { get; set; } = "";
    public bool IsStale { get; set; }
    public TimeSpan? TimeToLive { get; set; }
}

/// <summary>
/// Enhanced GraphQL execution response with comprehensive metadata
/// </summary>
public class GraphQLExecutionResponse
{
    public string QueryId { get; set; } = "";
    public object? Data { get; set; }
    public List<ExecutionError> Errors { get; set; } = [];
    public ExecutionMetadata Metadata { get; set; } = new();
    public QuerySuggestions? Suggestions { get; set; }
    public SchemaContext? SchemaContext { get; set; }
    public PerformanceRecommendations? Performance { get; set; }
    public SecurityAnalysis? Security { get; set; }
}

/// <summary>
/// Smart query suggestions
/// </summary>
public class QuerySuggestions
{
    public List<string> OptimizationHints { get; set; } = [];
    public List<QueryExample> RelatedQueries { get; set; } = [];
    public List<string> FieldSuggestions { get; set; } = [];
    public PaginationHints? PaginationHints { get; set; }
    public List<string> AlternativeApproaches { get; set; } = [];
}

/// <summary>
/// Schema context for query results
/// </summary>
public class SchemaContext
{
    public List<GraphQLTypeInfo> ReferencedTypes { get; set; } = [];
    public List<string> AvailableFields { get; set; } = [];
    public List<string> RequiredArguments { get; set; } = [];
    public Dictionary<string, List<string>> EnumValues { get; set; } = new();
    public List<string> RelatedOperations { get; set; } = [];
}

/// <summary>
/// Performance recommendations
/// </summary>
public class PerformanceRecommendations
{
    public bool ShouldCache { get; set; }
    public PaginationRecommendation? OptimalPagination { get; set; }
    public List<string> IndexHints { get; set; } = [];
    public QueryComplexityRating QueryComplexityRating { get; set; }
    public List<string> OptimizationSuggestions { get; set; } = [];
}

/// <summary>
/// Security analysis for queries
/// </summary>
public class SecurityAnalysis
{
    public List<string> SecurityWarnings { get; set; } = [];
    public List<string> RequiredPermissions { get; set; } = [];
    public bool HasSensitiveData { get; set; }
    public List<string> SecurityRecommendations { get; set; } = [];
}

/// <summary>
/// Batch execution response
/// </summary>
public class BatchExecutionResponse
{
    public string BatchId { get; set; } = "";
    public List<BatchQueryResult> Results { get; set; } = [];
    public BatchSummary Summary { get; set; } = new();
}

/// <summary>
/// Individual batch query result
/// </summary>
public class BatchQueryResult
{
    public int Index { get; set; }
    public string QueryId { get; set; } = "";
    public object? Data { get; set; }
    public List<string> Errors { get; set; } = [];
    public int ExecutionTimeMs { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// Batch execution summary
/// </summary>
public class BatchSummary
{
    public int TotalQueries { get; set; }
    public int SuccessfulQueries { get; set; }
    public int FailedQueries { get; set; }
    public int TotalExecutionTimeMs { get; set; }
    public double AverageQueryTimeMs { get; set; }
    public int MaxConcurrency { get; set; }
}

// Supporting types and enums

public enum TypeKind
{
    SCALAR, OBJECT, INTERFACE, UNION, ENUM, INPUT_OBJECT, LIST, NON_NULL
}

public enum QueryComplexityRating
{
    Simple, Moderate, Complex, VeryComplex
}

public class TypeReference
{
    public TypeKind? Kind { get; set; }
    public string? Name { get; set; }
    public TypeReference? OfType { get; set; }
}

public class ArgumentInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public TypeReference Type { get; set; } = new();
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public List<string> ValidValues { get; set; } = [];
}

public class InputFieldInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public TypeReference Type { get; set; } = new();
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
}

public class EnumValueInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsDeprecated { get; set; }
    public string? DeprecationReason { get; set; }
}

public class DirectiveInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Locations { get; set; } = [];
    public List<ArgumentInfo> Args { get; set; } = [];
}

public class SchemaInfo
{
    public TypeReference? QueryType { get; set; }
    public TypeReference? MutationType { get; set; }
    public TypeReference? SubscriptionType { get; set; }
    public DateTime LastModified { get; set; }
    public string Version { get; set; } = "";
}

public class SchemaMetadata
{
    public int TotalTypes { get; set; }
    public int TotalFields { get; set; }
    public DateTime LastIntrospected { get; set; }
    public List<string> Features { get; set; } = [];
    public Dictionary<string, int> TypeCounts { get; set; } = new();
}

public class TypeRelationships
{
    public Dictionary<string, List<string>> Implements { get; set; } = new();
    public Dictionary<string, List<string>> UsedBy { get; set; } = new();
    public Dictionary<string, List<string>> References { get; set; } = new();
}

public class UsageStatistics
{
    public int QueryCount { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime LastUsed { get; set; }
    public List<string> CommonUsagePatterns { get; set; } = [];
}

public class ComplexityMetrics
{
    public int FieldComplexity { get; set; }
    public int MaxDepth { get; set; }
    public int EstimatedCost { get; set; }
}

public class PerformanceProfile
{
    public TimeSpan AverageExecutionTime { get; set; }
    public int MemoryUsageMB { get; set; }
    public bool RequiresOptimization { get; set; }
}

public class SecurityInfo
{
    public List<string> RequiredRoles { get; set; } = [];
    public bool IsSensitive { get; set; }
    public List<string> SecurityNotes { get; set; } = [];
}

public class AuthenticationInfo
{
    public List<string> SupportedMethods { get; set; } = [];
    public bool Required { get; set; }
    public Dictionary<string, string> Configuration { get; set; } = new();
}

public class RateLimitInfo
{
    public int RequestsPerMinute { get; set; }
    public int RequestsPerHour { get; set; }
    public int CurrentUsage { get; set; }
    public DateTime ResetTime { get; set; }
}

public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime LastChecked { get; set; }
    public List<string> Issues { get; set; } = [];
}

public class VersionInfo
{
    public string Version { get; set; } = "";
    public string GraphQLVersion { get; set; } = "";
    public DateTime ReleaseDate { get; set; }
    public List<string> Features { get; set; } = [];
}

public class MemoryUsage
{
    public long UsedBytes { get; set; }
    public long AllocatedBytes { get; set; }
    public double EfficiencyRatio { get; set; }
}

public class PerformanceRecommendation
{
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public int Priority { get; set; }
    public string Implementation { get; set; } = "";
}

public class ExecutionError
{
    public string Message { get; set; } = "";
    public List<object> Path { get; set; } = [];
    public Dictionary<string, object> Extensions { get; set; } = new();
    public List<string> Suggestions { get; set; } = [];
}

public class ExecutionMetadata
{
    public int ExecutionTimeMs { get; set; }
    public int? ComplexityScore { get; set; }
    public int? DepthScore { get; set; }
    public int? FieldCount { get; set; }
    public bool CacheHit { get; set; }
    public bool Failed { get; set; }
    public DataFreshness? DataFreshness { get; set; }
}

public class DataFreshness
{
    public DateTime AsOf { get; set; }
    public bool IsStale { get; set; }
    public TimeSpan Age { get; set; }
}

public class PaginationInfo
{
    public bool SupportsCursor { get; set; }
    public bool SupportsOffset { get; set; }
    public int? DefaultLimit { get; set; }
    public int? MaxLimit { get; set; }
}

public class PaginationHints
{
    public bool ShouldPaginate { get; set; }
    public int RecommendedPageSize { get; set; }
    public string? PaginationStrategy { get; set; }
    public List<string> AvailableMethods { get; set; } = [];
}

public class PaginationRecommendation
{
    public string Method { get; set; } = "";
    public int RecommendedPageSize { get; set; }
    public string Reasoning { get; set; } = "";
}

public class BatchQueryRequest
{
    public string? Id { get; set; }
    public string Query { get; set; } = "";
    public Dictionary<string, object> Variables { get; set; } = new();
}
