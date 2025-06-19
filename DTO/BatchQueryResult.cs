namespace Graphql.Mcp.DTO;

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