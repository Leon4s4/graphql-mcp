namespace Graphql.Mcp.DTO;

public class QueryComplexityInfo
{
    public int Score { get; set; }
    public string Rating { get; set; } = "";
    public List<string> FactorsContributing { get; set; } = [];
}