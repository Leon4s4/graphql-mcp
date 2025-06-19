namespace Graphql.Mcp.DTO;

/// <summary>
/// Batch execution response
/// </summary>
public class BatchExecutionResponse
{
    public string BatchId { get; set; } = "";
    public List<BatchQueryResult> Results { get; set; } = [];
    public BatchSummary Summary { get; set; } = new();
}