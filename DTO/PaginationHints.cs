namespace Graphql.Mcp.DTO;

public class PaginationHints
{
    public bool ShouldPaginate { get; set; }
    public int RecommendedPageSize { get; set; }
    public string? PaginationStrategy { get; set; }
    public List<string> AvailableMethods { get; set; } = [];
}