namespace Graphql.Mcp.DTO;

public class SchemaMetadata
{
    public int TotalTypes { get; set; }
    public int TotalFields { get; set; }
    public DateTime LastIntrospected { get; set; }
    public List<string> Features { get; set; } = [];
    public Dictionary<string, int> TypeCounts { get; set; } = new();
}