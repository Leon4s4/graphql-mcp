namespace Graphql.Mcp.DTO;

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