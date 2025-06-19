namespace Graphql.Mcp.DTO;

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