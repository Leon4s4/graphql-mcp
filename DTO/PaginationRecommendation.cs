namespace Graphql.Mcp.DTO;

public class PaginationRecommendation
{
    public string Method { get; set; } = "";
    public int RecommendedPageSize { get; set; }
    public string Reasoning { get; set; } = "";
}