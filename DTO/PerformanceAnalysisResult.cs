namespace Graphql.Mcp.DTO;

/// <summary>
/// Performance analysis results
/// </summary>
public class PerformanceAnalysisResult
{
    public int ComplexityScore { get; set; }
    public string Rating { get; set; } = "";
    public string EstimatedTime { get; set; } = "";
    public List<string> Recommendations { get; set; } = [];
    public string Impact { get; set; } = "";
}