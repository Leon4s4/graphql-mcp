namespace Graphql.Mcp.DTO;

/// <summary>
/// Smart query suggestions
/// </summary>
public class QuerySuggestions
{
    public List<string> OptimizationHints { get; set; } = [];
    public List<QueryExample> RelatedQueries { get; set; } = [];
    public List<string> FieldSuggestions { get; set; } = [];
    public PaginationHints? PaginationHints { get; set; }
    public List<string> AlternativeApproaches { get; set; } = [];
}