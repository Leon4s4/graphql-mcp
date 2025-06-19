namespace Graphql.Mcp.DTO;

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