namespace Graphql.Mcp.DTO;

public class UsageStatistics
{
    public int QueryCount { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime LastUsed { get; set; }
    public List<string> CommonUsagePatterns { get; set; } = [];
}