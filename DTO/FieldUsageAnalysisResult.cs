namespace Graphql.Mcp.DTO;

/// <summary>
/// Field usage analysis results
/// </summary>
public class FieldUsageAnalysisResult
{
    public List<string> MostUsedFields { get; set; } = [];
    public List<string> UnusedFields { get; set; } = [];
    public Dictionary<string, int> UsageStats { get; set; } = new();
    public List<string> Recommendations { get; set; } = [];
}