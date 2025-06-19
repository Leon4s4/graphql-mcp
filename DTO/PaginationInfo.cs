namespace Graphql.Mcp.DTO;

public class PaginationInfo
{
    public bool SupportsCursor { get; set; }
    public bool SupportsOffset { get; set; }
    public int? DefaultLimit { get; set; }
    public int? MaxLimit { get; set; }
}