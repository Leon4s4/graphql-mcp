namespace Graphql.Mcp.DTO;

public class UtilityOptimizationResult
{
    public List<string> PerformanceOptimizations { get; set; } = [];
    public List<string> SizeOptimizations { get; set; } = [];
    public List<string> ReadabilityImprovements { get; set; } = [];
}