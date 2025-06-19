namespace Graphql.Mcp.DTO;

public class PerformanceRecommendation
{
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public int Priority { get; set; }
    public string Implementation { get; set; } = "";
}