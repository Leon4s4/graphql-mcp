namespace Graphql.Mcp.DTO;

public class DataFreshness
{
    public DateTime AsOf { get; set; }
    public bool IsStale { get; set; }
    public TimeSpan Age { get; set; }
}