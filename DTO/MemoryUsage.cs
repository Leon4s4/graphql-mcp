namespace Graphql.Mcp.DTO;

public class MemoryUsage
{
    public long UsedBytes { get; set; }
    public long AllocatedBytes { get; set; }
    public double EfficiencyRatio { get; set; }
}