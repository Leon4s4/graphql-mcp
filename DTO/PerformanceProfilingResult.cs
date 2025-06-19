namespace Graphql.Mcp.DTO;

public class PerformanceProfilingResult
{
    public string EstimatedTime { get; set; } = "";
    public string Impact { get; set; } = "";
    public List<string> Bottlenecks { get; set; } = [];
    public List<string> Optimizations { get; set; } = [];
}