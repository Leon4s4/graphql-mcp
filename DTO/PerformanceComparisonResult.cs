namespace Graphql.Mcp.DTO;

/// <summary>
/// Performance comparison between two endpoints
/// </summary>
public class PerformanceComparisonResult
{
    /// <summary>
    /// Average response time for endpoint 1 (ms)
    /// </summary>
    public double Endpoint1AvgResponseTime { get; set; }

    /// <summary>
    /// Average response time for endpoint 2 (ms)
    /// </summary>
    public double Endpoint2AvgResponseTime { get; set; }

    /// <summary>
    /// Success rate for endpoint 1 (0-1)
    /// </summary>
    public double Endpoint1SuccessRate { get; set; }

    /// <summary>
    /// Success rate for endpoint 2 (0-1)
    /// </summary>
    public double Endpoint2SuccessRate { get; set; }

    /// <summary>
    /// Which endpoint performs better
    /// </summary>
    public string BetterPerformer { get; set; } = "Similar";

    /// <summary>
    /// Performance difference percentage
    /// </summary>
    public double PerformanceDifferencePercent { get; set; }

    /// <summary>
    /// Reliability comparison
    /// </summary>
    public string ReliabilityComparison { get; set; } = "Similar";

    /// <summary>
    /// Recommendations based on performance comparison
    /// </summary>
    public List<string> PerformanceRecommendations { get; set; } = new();
}