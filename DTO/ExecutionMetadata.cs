namespace Graphql.Mcp.DTO;

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