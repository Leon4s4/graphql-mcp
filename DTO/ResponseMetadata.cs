namespace Graphql.Mcp.DTO;

/// <summary>
/// Response metadata for comprehensive responses
/// </summary>
public class ResponseMetadata
{
    public TimeSpan ProcessingTime { get; set; }
    public string CacheStatus { get; set; } = "";
    public string OperationType { get; set; } = "";
    public List<string> RecommendedActions { get; set; } = [];
    public List<string> RelatedEndpoints { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}