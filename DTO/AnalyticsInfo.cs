namespace Graphql.Mcp.DTO;

/// <summary>
/// Analytics information for comprehensive responses
/// </summary>
public class AnalyticsInfo
{
    public string ComplexityRating { get; set; } = "";
    public string PerformanceImpact { get; set; } = "";
    public string ResourceUsage { get; set; } = "";
    public List<string> RecommendedNextSteps { get; set; } = [];
}