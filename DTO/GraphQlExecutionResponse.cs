namespace Graphql.Mcp.DTO;

/// <summary>
/// Enhanced GraphQL execution response with comprehensive metadata
/// </summary>
public class GraphQlExecutionResponse
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