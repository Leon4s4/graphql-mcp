namespace Graphql.Mcp.DTO;

public class PerformanceProfile
{
    public TimeSpan AverageExecutionTime { get; set; }
    public int MemoryUsageMb { get; set; }
    public bool RequiresOptimization { get; set; }
}