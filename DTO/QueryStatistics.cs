namespace Graphql.Mcp.DTO;

/// <summary>
/// Query statistics information
/// </summary>
public class QueryStatistics
{
    public int ExecutionCount { get; set; }
    public string AverageTime { get; set; } = "";
    public string LastExecuted { get; set; } = "";
    public List<string> CommonErrors { get; set; } = [];
}