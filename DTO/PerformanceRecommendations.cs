namespace Graphql.Mcp.DTO;

/// <summary>
/// Performance recommendations
/// </summary>
public class PerformanceRecommendations
{
    public bool ShouldCache { get; set; }
    public PaginationRecommendation? OptimalPagination { get; set; }
    public List<string> IndexHints { get; set; } = [];
    public QueryComplexityRating QueryComplexityRating { get; set; }
    public List<string> OptimizationSuggestions { get; set; } = [];
}