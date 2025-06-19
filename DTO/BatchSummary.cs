namespace Graphql.Mcp.DTO;

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