namespace Graphql.Mcp.DTO;

public class TypeRelationships
{
    public Dictionary<string, List<string>> Implements { get; set; } = new();
    public Dictionary<string, List<string>> UsedBy { get; set; } = new();
    public Dictionary<string, List<string>> References { get; set; } = new();
}