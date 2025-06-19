namespace Graphql.Mcp.DTO;

/// <summary>
/// Comprehensive GraphQL response with smart defaults and metadata
/// </summary>
public class GraphQlComprehensiveResponse
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

// Supporting types and enums

// Additional DTO classes for replacing object returns